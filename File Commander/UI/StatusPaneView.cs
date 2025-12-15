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
    private ListView _jobListView = null!;
    private ListView _historyListView = null!;
    private Label _infoLabel = null!;

    private readonly List<string> _commandHistory = new();
    private const int MAX_HISTORY = 100;

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
        Height = 3;

        _tabView = new TabView
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        // Tab 1: Job Queue/Status
        var jobTab = new TabView.Tab("Jobs", CreateJobView());

        // Tab 2: Command History
        var historyTab = new TabView.Tab("History", CreateHistoryView());

        // Tab 3: Overview/Info
        var infoTab = new TabView.Tab("Info", CreateInfoView());

        _tabView.AddTab(jobTab, false);
        _tabView.AddTab(historyTab, false);
        _tabView.AddTab(infoTab, false);

        Add(_tabView);
    }

    private View CreateJobView()
    {
        var view = new View
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        _jobListView = new ListView
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        view.Add(_jobListView);
        return view;
    }

    private View CreateHistoryView()
    {
        var view = new View
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        _historyListView = new ListView
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        view.Add(_historyListView);
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
        // Subscribe to task queue events
        _taskQueue.JobQueued += (s, job) => UpdateJobList();
        _taskQueue.JobStarted += (s, job) => UpdateJobList();
        _taskQueue.JobCompleted += (s, job) =>
        {
            AddCommandHistory($"✓ Job completed: {job.Operation} {Path.GetFileName(job.SourcePath)}");
            UpdateJobList();
        };
        _taskQueue.JobFailed += (s, job) =>
        {
            AddCommandHistory($"✗ Job failed: {job.Operation} {Path.GetFileName(job.SourcePath)} - {job.ErrorMessage}");
            UpdateJobList();
        };

        // Subscribe to progress updates for live display
        _taskQueue.ProgressChanged += (s, progressData) =>
        {
            UpdateJobList(); // Refresh to show updated progress
        };
    }

    /// <summary>
    /// Updates the job list display - shows only active jobs (queued/running)
    /// </summary>
    private void UpdateJobList()
    {
        var jobs = _taskQueue.ActiveJobs
            .Where(j => j.Status == JobStatus.Queued || j.Status == JobStatus.Running)
            .ToList();

        var displayItems = jobs.Select(j =>
            $"[{j.Status}] {j.Operation} {Path.GetFileName(j.SourcePath)} ({j.Progress}%)"
        ).ToList();

        if (displayItems.Count == 0)
        {
            displayItems.Add("No active jobs");
        }

        Terminal.Gui.Application.MainLoop.Invoke(() =>
        {
            _jobListView.SetSource(displayItems);
        });
    }

    /// <summary>
    /// Adds a command to the history
    /// </summary>
    public void AddCommandHistory(string command)
    {
        _commandHistory.Insert(0, $"{DateTime.Now:HH:mm:ss} - {command}");

        if (_commandHistory.Count > MAX_HISTORY)
        {
            _commandHistory.RemoveRange(MAX_HISTORY, _commandHistory.Count - MAX_HISTORY);
        }

        _historyListView.SetSource(_commandHistory);
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

    private int _currentHeight = 3;

    /// <summary>
    /// Resizes the status pane (toggle between compact and expanded)
    /// </summary>
    public void ToggleSize()
    {
        if (_currentHeight == 3)
        {
            _currentHeight = 8;
            Height = 8;
        }
        else
        {
            _currentHeight = 3;
            Height = 3;
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

