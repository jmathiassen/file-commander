namespace File_Commander.Models;

/// <summary>
/// Represents the status of a file operation job
/// </summary>
public enum JobStatus
{
    Queued,
    Running,
    Completed,
    Failed,
    Cancelled
}

/// <summary>
/// Represents a single file operation job in the queue
/// </summary>
public class FileOperationJob
{
    public Guid JobId { get; set; } = Guid.NewGuid();
    public OperationType Operation { get; set; }
    public string SourcePath { get; set; } = string.Empty;
    public string DestinationPath { get; set; } = string.Empty;
    public JobStatus Status { get; set; } = JobStatus.Queued;
    public int Progress { get; set; } // 0-100
    public string? ErrorMessage { get; set; }
    public DateTime QueuedTime { get; set; } = DateTime.Now;
    public DateTime? StartedTime { get; set; }
    public DateTime? CompletedTime { get; set; }

    /// <summary>
    /// Gets the drive pair key for queuing logic (source:destination)
    /// </summary>
    public string GetDrivePairKey()
    {
        var sourceDrive = Path.GetPathRoot(SourcePath) ?? "unknown";
        var destDrive = Path.GetPathRoot(DestinationPath) ?? "unknown";
        return $"{sourceDrive}:{destDrive}";
    }
}

