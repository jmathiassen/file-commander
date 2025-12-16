using Terminal.Gui;
using File_Commander.Services;
using File_Commander.Models;

namespace File_Commander.UI;

/// <summary>
/// Tabbed status pane for job monitoring, command history, and system info
/// Phase 3: Complex monitoring and history tracking
/// </summary>
public class StatusPaneView : FrameView
{
    private readonly IntelligentTaskQueueService _taskQueue;
    private TabView _tabView = null!;
    private ListView _jobQueueListView = null!;
    private ListView _activityLogListView = null!;
    private Label _infoLabel = null!;

    private readonly List<string> _activityLog = new();
    private const int MAX_LOG_ENTRIES = 200;

    public StatusPaneView(IntelligentTaskQueueService taskQueue) : base("Status")
    {
        _taskQueue = taskQueue;
        InitializeUI();
        SetupEventHandlers();
    }

    private void InitializeUI()
    {
        X = 0;
        Width = Dim.Fill();
        Height = 6; // Increased default height for better visibility

        _tabView = new TabView
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        // Tab 1: Job Queue - Shows queued and running jobs
        var jobQueueTab = new TabView.Tab("Job Queue", CreateJobQueueView());

        // Tab 2: Activity Log - Shows all operations, refreshes, errors
        var activityLogTab = new TabView.Tab("Activity Log", CreateActivityLogView());

        // Tab 3: System Info
        var infoTab = new TabView.Tab("Info", CreateInfoView());

        _tabView.AddTab(jobQueueTab, false);
        _tabView.AddTab(activityLogTab, false);
        _tabView.AddTab(infoTab, false);

        Add(_tabView);
    }

    private View CreateJobQueueView()
    {
        var view = new View
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        // Control buttons at the top
        var pauseButton = new Button("Pause (Ctrl+P)")
        {
            X = 0,
            Y = 0
        };
        pauseButton.Clicked += () => _taskQueue.PauseQueue();

        var resumeButton = new Button("Resume (Ctrl+R)")
        {
            X = Pos.Right(pauseButton) + 2,
            Y = 0
        };
        resumeButton.Clicked += () => _taskQueue.ResumeQueue();

        var clearButton = new Button("Clear Queue")
        {
            X = Pos.Right(resumeButton) + 2,
            Y = 0
        };
        clearButton.Clicked += () => _taskQueue.ClearQueue();

        var statusLabel = new Label("Queue: Running")
        {
            X = Pos.Right(clearButton) + 4,
            Y = 0
        };

        // Update status label when queue state changes
        _taskQueue.QueueStateChanged += (s, isPaused) =>
        {
            Terminal.Gui.Application.MainLoop?.Invoke(() =>
            {
                statusLabel.Text = isPaused ? "Queue: PAUSED" : "Queue: Running";
                statusLabel.ColorScheme = isPaused
                    ? new ColorScheme { Normal = new Terminal.Gui.Attribute(Color.BrightYellow, Color.Black) }
                    : Colors.Base;
            });
        };

        _jobQueueListView = new ListView
        {
            X = 0,
            Y = 1,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        view.Add(pauseButton, resumeButton, clearButton, statusLabel, _jobQueueListView);
        return view;
    }

    private View CreateActivityLogView()
    {
        var view = new View
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        _activityLogListView = new ListView
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        view.Add(_activityLogListView);
        return view;
    }

    private View CreateInfoView()
    {
        var view = new View
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        _infoLabel = new Label
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            Text = "System Information"
        };

        view.Add(_infoLabel);
        return view;
    }

    private void SetupEventHandlers()
    {
        // Subscribe to task queue events for job queue display
        _taskQueue.JobQueued += (s, job) =>
        {
            LogActivity($"Job queued: {job.Operation} {Path.GetFileName(job.SourcePath)}");
            UpdateJobQueue();
        };

        _taskQueue.JobStarted += (s, job) =>
        {
            LogActivity($"Job started: {job.Operation} {Path.GetFileName(job.SourcePath)}");
            UpdateJobQueue();
        };

        _taskQueue.JobCompleted += (s, job) =>
        {
            LogActivity($"✓ Completed: {job.Operation} {Path.GetFileName(job.SourcePath)}");
            UpdateJobQueue();
        };

        _taskQueue.JobFailed += (s, job) =>
        {
            LogActivity($"✗ Failed: {job.Operation} {Path.GetFileName(job.SourcePath)} - {job.ErrorMessage}");
            UpdateJobQueue();
        };

        // Subscribe to progress updates for live display
        _taskQueue.ProgressChanged += (s, progressData) =>
        {
            UpdateJobQueue(); // Refresh to show updated progress
        };
    }

    /// <summary>
    /// Updates the job queue display - shows only queued and running jobs
    /// </summary>
    private void UpdateJobQueue()
    {
        var jobs = _taskQueue.ActiveJobs
            .Where(j => j.Status == JobStatus.Queued || j.Status == JobStatus.Running)
            .ToList();

        var displayItems = jobs.Select(j =>
        {
            var progressBar = CreateProgressBar(j.Progress);
            return $"[{j.Status,-8}] {j.Operation,-6} {Path.GetFileName(j.SourcePath),-30} {progressBar} {j.Progress}%";
        }).ToList();

        if (displayItems.Count == 0)
        {
            displayItems.Add("No active jobs");
        }

        Terminal.Gui.Application.MainLoop.Invoke(() =>
        {
            _jobQueueListView.SetSource(displayItems);
        });
    }

    /// <summary>
    /// Creates a simple ASCII progress bar
    /// </summary>
    private string CreateProgressBar(int progress)
    {
        const int barWidth = 20;
        var filled = (int)(progress / 100.0 * barWidth);
        var empty = barWidth - filled;
        return $"[{new string('█', filled)}{new string('░', empty)}]";
    }

    /// <summary>
    /// Adds an entry to the activity log with timestamp
    /// </summary>
    public void LogActivity(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        _activityLog.Insert(0, $"{timestamp} │ {message}");

        if (_activityLog.Count > MAX_LOG_ENTRIES)
        {
            _activityLog.RemoveRange(MAX_LOG_ENTRIES, _activityLog.Count - MAX_LOG_ENTRIES);
        }

        Terminal.Gui.Application.MainLoop?.Invoke(() =>
        {
            _activityLogListView.SetSource(_activityLog);
        });
    }

    /// <summary>
    /// Adds a command to the activity log (for backwards compatibility)
    /// </summary>
    public void AddCommandHistory(string command)
    {
        LogActivity(command);
    }

    /// <summary>
    /// Updates the info display
    /// </summary>
    public void UpdateInfo(string markedCount, string activeDir, long? activeDirSize)
    {
        var sizeStr = activeDirSize.HasValue ? FormatBytes(activeDirSize.Value) : "calculating...";

        var info = $"Marked Files: {markedCount}\n" +
                   $"Active Directory: {activeDir}\n" +
                   $"Directory Size: {sizeStr}\n" +
                   $"Memory: {GC.GetTotalMemory(false) / 1024 / 1024} MB";

        _infoLabel.Text = info;
    }

    /// <summary>
    /// Switches to the next tab
    /// </summary>
    public void SwitchToNextTab()
    {
        // Get current tab index
        var currentTab = _tabView.SelectedTab;
        var currentIndex = 0;

        for (int i = 0; i < _tabView.Tabs.Count; i++)
        {
            if (_tabView.Tabs.ElementAt(i) == currentTab)
            {
                currentIndex = i;
                break;
            }
        }

        var nextIndex = (currentIndex + 1) % _tabView.Tabs.Count;
        _tabView.SelectedTab = _tabView.Tabs.ElementAt(nextIndex);
    }

    private int _currentHeight = 6;

    /// <summary>
    /// Resizes the status pane (toggle between normal and expanded)
    /// </summary>
    public void ToggleSize()
    {
        if (_currentHeight == 6)
        {
            _currentHeight = 12;
            Height = 12;
        }
        else
        {
            _currentHeight = 6;
            Height = 6;
        }
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double size = bytes;

        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }

        return $"{size:0.##} {sizes[order]}";
    }
}

