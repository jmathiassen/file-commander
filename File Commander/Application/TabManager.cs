using File_Commander.Models;
using File_Commander.Services;

namespace File_Commander.Application;

/// <summary>
/// Manages tabs and their states
/// </summary>
public class TabManager
{
    private readonly FileSystemService _fileSystemService;
    private readonly ConfigService _configService;
    private readonly List<TabState> _tabs = new();
    private int _activeTabIndex = 0;
    private readonly Dictionary<string, FileSystemWatcher> _watchers = new();

    public TabState ActiveTab => _tabs[_activeTabIndex];
    public IReadOnlyList<TabState> Tabs => _tabs.AsReadOnly();
    public int ActiveTabIndex => _activeTabIndex;

    public event EventHandler? TabChanged;
    public event EventHandler? TabStateChanged;
    public event EventHandler<string>? DirectoryRefreshed;

    public TabManager(FileSystemService fileSystemService, ConfigService configService)
    {
        _fileSystemService = fileSystemService;
        _configService = configService;

        // Subscribe to settings changes to update watcher
        _configService.SettingsChanged += (s, settings) => SetupDirectoryWatcher(settings);

        // Subscribe to tab changes to update watcher for ActiveTabOnly mode
        TabChanged += (s, e) =>
        {
            if (_configService.Settings.DirectoryUpdateMode == DirectoryUpdateMode.ActiveTabOnly)
            {
                SetupDirectoryWatcher(_configService.Settings);
            }
        };
    }

    /// <summary>
    /// Creates a new tab with the specified path
    /// </summary>
    public void CreateTab(string path)
    {
        var normalizedPath = _fileSystemService.NormalizePath(path);
        var tab = new TabState(normalizedPath);
        _tabs.Add(tab);

        if (_tabs.Count == 1)
        {
            _activeTabIndex = 0;
        }

        TabChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Switches to the specified tab index
    /// </summary>
    public void SwitchToTab(int index)
    {
        if (index >= 0 && index < _tabs.Count)
        {
            _activeTabIndex = index;
            TabChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Closes the specified tab
    /// </summary>
    public void CloseTab(int index)
    {
        if (_tabs.Count <= 1)
        {
            return; // Keep at least one tab
        }

        if (index >= 0 && index < _tabs.Count)
        {
            _tabs.RemoveAt(index);

            if (_activeTabIndex >= _tabs.Count)
            {
                _activeTabIndex = _tabs.Count - 1;
            }

            TabChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Refreshes the file list for the active pane
    /// </summary>
    public void RefreshActivePane()
    {
        var tab = ActiveTab;
        var showHidden = _configService.Settings.ShowHiddenFiles;

        if (tab.DisplayMode == DisplayMode.SinglePane || tab.IsLeftPaneActive)
        {
            tab.FilesActive = _fileSystemService.ListDirectory(tab.CurrentPath, showHidden);
            tab.SelectedIndexActive = Math.Min(tab.SelectedIndexActive, Math.Max(0, tab.FilesActive.Count - 1));
        }
        else
        {
            tab.FilesPassive = _fileSystemService.ListDirectory(tab.PathPassive, showHidden);
            tab.SelectedIndexPassive = Math.Min(tab.SelectedIndexPassive, Math.Max(0, tab.FilesPassive.Count - 1));
        }

        tab.IsDirty = false;
        DirectoryRefreshed?.Invoke(this, tab.IsLeftPaneActive ? tab.CurrentPath : tab.PathPassive);
        TabStateChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Refreshes both panes (for dual pane mode)
    /// </summary>
    public void RefreshBothPanes()
    {
        var tab = ActiveTab;
        var showHidden = _configService.Settings.ShowHiddenFiles;

        tab.FilesActive = _fileSystemService.ListDirectory(tab.CurrentPath, showHidden);
        tab.SelectedIndexActive = Math.Min(tab.SelectedIndexActive, Math.Max(0, tab.FilesActive.Count - 1));

        if (tab.DisplayMode == DisplayMode.DualPane)
        {
            tab.FilesPassive = _fileSystemService.ListDirectory(tab.PathPassive, showHidden);
            tab.SelectedIndexPassive = Math.Min(tab.SelectedIndexPassive, Math.Max(0, tab.FilesPassive.Count - 1));
        }

        tab.IsDirty = false;
        DirectoryRefreshed?.Invoke(this, $"{tab.CurrentPath} and {tab.PathPassive}");
        TabStateChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Navigates to a new path in the active pane
    /// </summary>
    public void NavigateTo(string path)
    {
        var tab = ActiveTab;
        var normalizedPath = _fileSystemService.NormalizePath(path);

        // Remember the previous directory name to position cursor on it
        string? previousDirName = null;

        if (tab.DisplayMode == DisplayMode.SinglePane || tab.IsLeftPaneActive)
        {
            // Get the name of the current directory before changing
            previousDirName = Path.GetFileName(tab.CurrentPath);

            tab.CurrentPath = normalizedPath;
            tab.SelectedIndexActive = 0;
            tab.ScrollOffsetActive = 0;
        }
        else
        {
            // Get the name of the current directory before changing
            previousDirName = Path.GetFileName(tab.PathPassive);

            tab.PathPassive = normalizedPath;
            tab.SelectedIndexPassive = 0;
            tab.ScrollOffsetPassive = 0;
        }

        RefreshActivePane();

        // If we navigated up (to parent), position cursor on the directory we just exited
        if (!string.IsNullOrEmpty(previousDirName))
        {
            var files = tab.IsLeftPaneActive ? tab.FilesActive : tab.FilesPassive;
            var indexOfPrevious = files.FindIndex(f => f.Name == previousDirName);

            if (indexOfPrevious >= 0)
            {
                if (tab.DisplayMode == DisplayMode.SinglePane || tab.IsLeftPaneActive)
                {
                    tab.SelectedIndexActive = indexOfPrevious;
                }
                else
                {
                    tab.SelectedIndexPassive = indexOfPrevious;
                }

                // Notify to update UI
                TabStateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        // Re-setup watcher for the new directory
        SetupDirectoryWatcher(_configService.Settings);
    }

    /// <summary>
    /// Switches the active pane (in dual pane mode)
    /// </summary>
    public void SwitchActivePane()
    {
        var tab = ActiveTab;

        if (tab.DisplayMode == DisplayMode.DualPane)
        {
            tab.IsLeftPaneActive = !tab.IsLeftPaneActive;
            TabStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Toggles display mode between single and dual pane
    /// </summary>
    public void ToggleDisplayMode()
    {
        var tab = ActiveTab;

        tab.DisplayMode = tab.DisplayMode == DisplayMode.SinglePane
            ? DisplayMode.DualPane
            : DisplayMode.SinglePane;

        if (tab.DisplayMode == DisplayMode.DualPane && tab.FilesPassive.Count == 0)
        {
            RefreshBothPanes();
        }

        TabStateChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Calculates and updates directory size for a specific file item
    /// </summary>
    public void CalculateDirectorySize(FileItem item)
    {
        if (!item.IsDirectory || item.Name == "..")
        {
            return;
        }

        // Calculate size asynchronously to avoid blocking UI
        Task.Run(() =>
        {
            var size = _fileSystemService.CalculateDirectorySize(item.FullPath);
            item.CalculatedSize = size;

            // Trigger UI update
            TabStateChanged?.Invoke(this, EventArgs.Empty);
        });
    }

    /// <summary>
    /// Notifies subscribers that tab state has changed (for external use)
    /// </summary>
    public void NotifyStateChanged()
    {
        TabStateChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Sets up directory watcher based on configuration settings
    /// </summary>
    private void SetupDirectoryWatcher(AppSettings settings)
    {
        // Dispose all existing watchers
        foreach (var watcher in _watchers.Values)
        {
            watcher.Dispose();
        }
        _watchers.Clear();

        // Don't create watcher if manual mode
        if (settings.DirectoryUpdateMode == DirectoryUpdateMode.Manual)
        {
            return;
        }

        // Don't create watcher if no tabs exist yet
        if (_tabs.Count == 0)
        {
            return;
        }

        var tab = ActiveTab;

        // Collect unique paths to watch
        var pathsToWatch = new HashSet<string>();

        // Always watch the current (left) pane
        pathsToWatch.Add(tab.CurrentPath);

        // In dual-pane mode, also watch the right pane if it's a different path
        if (tab.DisplayMode == DisplayMode.DualPane && tab.PathPassive != tab.CurrentPath)
        {
            pathsToWatch.Add(tab.PathPassive);
        }

        // Create watchers for each unique path
        foreach (var path in pathsToWatch)
        {
            CreateWatcher(path);
        }
    }

    /// <summary>
    /// Creates a FileSystemWatcher for the specified path
    /// </summary>
    private void CreateWatcher(string watchPath)
    {
        if (!Directory.Exists(watchPath))
        {
            return;
        }

        // Don't create duplicate watcher
        if (_watchers.ContainsKey(watchPath))
        {
            return;
        }

        try
        {
            var watcher = new FileSystemWatcher(watchPath)
            {
                NotifyFilter = NotifyFilters.FileName |
                              NotifyFilters.DirectoryName |
                              NotifyFilters.LastWrite |
                              NotifyFilters.Size,
                EnableRaisingEvents = true,
                IncludeSubdirectories = false
            };

            // Capture the path in the lambda to identify which directory changed
            var capturedPath = watchPath;
            watcher.Changed += (s, e) => OnDirectoryChanged(e, capturedPath, "modified");
            watcher.Created += (s, e) => OnDirectoryChanged(e, capturedPath, "created");
            watcher.Deleted += (s, e) => OnDirectoryChanged(e, capturedPath, "deleted");
            watcher.Renamed += (s, e) => OnDirectoryRenamed(e, capturedPath);

            _watchers[watchPath] = watcher;
        }
        catch (Exception)
        {
            // Ignore errors setting up watcher (e.g., permission denied)
        }
    }

    /// <summary>
    /// Handles directory change events from FileSystemWatcher
    /// </summary>
    private void OnDirectoryChanged(FileSystemEventArgs e, string watchedPath, string changeType)
    {
        // Use MainLoop.Invoke to safely update UI from background thread
        Terminal.Gui.Application.MainLoop?.Invoke(() =>
        {
            var tab = ActiveTab;
            var fileName = Path.GetFileName(e.FullPath);
            var isDirectory = Directory.Exists(e.FullPath);
            var itemType = isDirectory ? "directory" : "file";

            // Determine which pane(s) this path belongs to
            var panes = new List<string>();
            if (watchedPath == tab.CurrentPath)
            {
                panes.Add("left");
            }
            if (tab.DisplayMode == DisplayMode.DualPane && watchedPath == tab.PathPassive && watchedPath != tab.CurrentPath)
            {
                panes.Add("right");
            }

            // Log the change with pane identification
            var paneDesc = panes.Count > 0 ? $"[{string.Join("/", panes)} pane] " : "";
            DirectoryRefreshed?.Invoke(this,
                $"{paneDesc}{itemType} {changeType}: {fileName}");

            // Refresh appropriate pane(s)
            if (tab.DisplayMode == DisplayMode.DualPane)
            {
                RefreshBothPanes();
            }
            else
            {
                RefreshActivePane();
            }
        });
    }

    /// <summary>
    /// Handles file/directory rename events
    /// </summary>
    private void OnDirectoryRenamed(RenamedEventArgs e, string watchedPath)
    {
        Terminal.Gui.Application.MainLoop?.Invoke(() =>
        {
            var tab = ActiveTab;
            var oldName = Path.GetFileName(e.OldFullPath);
            var newName = Path.GetFileName(e.FullPath);
            var isDirectory = Directory.Exists(e.FullPath);
            var itemType = isDirectory ? "directory" : "file";

            // Determine which pane(s) this path belongs to
            var panes = new List<string>();
            if (watchedPath == tab.CurrentPath)
            {
                panes.Add("left");
            }
            if (tab.DisplayMode == DisplayMode.DualPane && watchedPath == tab.PathPassive && watchedPath != tab.CurrentPath)
            {
                panes.Add("right");
            }

            // Log the rename with pane identification
            var paneDesc = panes.Count > 0 ? $"[{string.Join("/", panes)} pane] " : "";
            DirectoryRefreshed?.Invoke(this,
                $"{paneDesc}{itemType} renamed: {oldName} → {newName}");

            // Refresh appropriate pane(s)
            if (tab.DisplayMode == DisplayMode.DualPane)
            {
                RefreshBothPanes();
            }
            else
            {
                RefreshActivePane();
            }
        });
    }

    /// <summary>
    /// Initializes the directory watcher for the first time
    /// </summary>
    public void InitializeWatcher()
    {
        SetupDirectoryWatcher(_configService.Settings);
    }
}

