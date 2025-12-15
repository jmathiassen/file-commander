using System.Collections.Concurrent;
using System.Threading.Channels;
using File_Commander.Models;

namespace File_Commander.Services;

/// <summary>
/// Intelligent task queue with drive-aware parallelism
/// Phase 2: Jobs on same drive pair run sequentially, different pairs run in parallel
/// </summary>
public class IntelligentTaskQueueService
{
    private readonly Channel<FileOperationJob> _jobQueue;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _drivePairLocks;
    private readonly ConcurrentDictionary<Guid, FileOperationJob> _activeJobs;
    private readonly FileOperationExecutor _executor;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task _processingTask;

    public event EventHandler<FileOperationJob>? JobQueued;
    public event EventHandler<FileOperationJob>? JobStarted;
    public event EventHandler<FileOperationJob>? JobCompleted;
    public event EventHandler<FileOperationJob>? JobFailed;
    public event EventHandler<(Guid JobId, int Progress)>? ProgressChanged;

    public IEnumerable<FileOperationJob> ActiveJobs => _activeJobs.Values;

    public IntelligentTaskQueueService(FileOperationExecutor executor)
    {
        _executor = executor;
        _jobQueue = Channel.CreateUnbounded<FileOperationJob>();
        _drivePairLocks = new ConcurrentDictionary<string, SemaphoreSlim>();
        _activeJobs = new ConcurrentDictionary<Guid, FileOperationJob>();
        _cancellationTokenSource = new CancellationTokenSource();

        // Subscribe to executor events and forward with job context
        _executor.ProgressChanged += (s, progressData) =>
        {
            // Forward progress events
            ProgressChanged?.Invoke(this, progressData);

            // Update job progress
            if (_activeJobs.TryGetValue(progressData.JobId, out var job))
            {
                job.Progress = progressData.Progress;
            }
        };

        _executor.StatusChanged += (s, statusData) =>
        {
            // Update job status
            if (_activeJobs.TryGetValue(statusData.JobId, out var job))
            {
                // Update status message if needed
            }
        };

        // Start background processing
        _processingTask = Task.Run(() => ProcessJobsAsync(_cancellationTokenSource.Token));
    }

    /// <summary>
    /// Enqueues a new file operation job
    /// </summary>
    public async Task<Guid> EnqueueAsync(FileOperationJob job)
    {
        job.Status = JobStatus.Queued;
        job.QueuedTime = DateTime.Now;

        await _jobQueue.Writer.WriteAsync(job);
        _activeJobs.TryAdd(job.JobId, job);

        JobQueued?.Invoke(this, job);

        return job.JobId;
    }

    /// <summary>
    /// Cancels a specific job
    /// </summary>
    public void CancelJob(Guid jobId)
    {
        if (_activeJobs.TryGetValue(jobId, out var job))
        {
            job.Status = JobStatus.Cancelled;
            // Note: Need to implement cancellation token per job
        }
    }

    /// <summary>
    /// Main processing loop - implements drive-aware parallelism
    /// </summary>
    private async Task ProcessJobsAsync(CancellationToken cancellationToken)
    {
        var processingTasks = new List<Task>();

        await foreach (var job in _jobQueue.Reader.ReadAllAsync(cancellationToken))
        {
            // Don't await - let it run in parallel (drive lock will serialize same-pair jobs)
            var task = ProcessSingleJobAsync(job, cancellationToken);
            processingTasks.Add(task);

            // Clean up completed tasks periodically
            processingTasks.RemoveAll(t => t.IsCompleted);
        }

        // Wait for all jobs to complete on shutdown
        await Task.WhenAll(processingTasks);
    }

    /// <summary>
    /// Processes a single job with drive-aware locking
    /// </summary>
    private async Task ProcessSingleJobAsync(FileOperationJob job, CancellationToken cancellationToken)
    {
        var drivePairKey = job.GetDrivePairKey();

        // Get or create a semaphore for this drive pair (ensures sequential processing per pair)
        var driveLock = _drivePairLocks.GetOrAdd(drivePairKey, _ => new SemaphoreSlim(1, 1));

        try
        {
            // CRITICAL: Wait for the drive pair lock (serializes jobs on same drive pair)
            await driveLock.WaitAsync(cancellationToken);

            try
            {
                job.Status = JobStatus.Running;
                job.StartedTime = DateTime.Now;
                JobStarted?.Invoke(this, job);

                // Execute the job based on operation type
                bool success = job.Operation switch
                {
                    OperationType.Copy => await _executor.CopySingleAsync(job.JobId, job.SourcePath, job.DestinationPath, cancellationToken),
                    OperationType.Move => await _executor.MoveSingleAsync(job.JobId, job.SourcePath, job.DestinationPath, cancellationToken),
                    OperationType.Delete => await _executor.DeleteSingleAsync(job.JobId, job.SourcePath, cancellationToken),
                    _ => throw new InvalidOperationException($"Unknown operation type: {job.Operation}")
                };

                if (success)
                {
                    job.Status = JobStatus.Completed;
                    job.Progress = 100;
                    job.CompletedTime = DateTime.Now;
                    JobCompleted?.Invoke(this, job);
                }
                else
                {
                    job.Status = JobStatus.Failed;
                    JobFailed?.Invoke(this, job);
                }
            }
            finally
            {
                // CRITICAL: Always release the drive pair lock
                driveLock.Release();
            }
        }
        catch (OperationCanceledException)
        {
            job.Status = JobStatus.Cancelled;
        }
        catch (Exception ex)
        {
            job.Status = JobStatus.Failed;
            job.ErrorMessage = ex.Message;
            JobFailed?.Invoke(this, job);
        }
        finally
        {
            _activeJobs.TryRemove(job.JobId, out _);
        }
    }

    /// <summary>
    /// Shuts down the queue and waits for active jobs to complete
    /// </summary>
    public async Task ShutdownAsync()
    {
        _jobQueue.Writer.Complete();
        _cancellationTokenSource.Cancel();

        try
        {
            await _processingTask;
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown
        }

        _cancellationTokenSource.Dispose();
    }
}

