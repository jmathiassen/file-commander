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

    public TabState ActiveTab => _tabs[_activeTabIndex];
    public IReadOnlyList<TabState> Tabs => _tabs.AsReadOnly();
    public int ActiveTabIndex => _activeTabIndex;

    public event EventHandler? TabChanged;
    public event EventHandler? TabStateChanged;

    public TabManager(FileSystemService fileSystemService, ConfigService configService)
    {
        _fileSystemService = fileSystemService;
        _configService = configService;
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

        if (tab.DisplayMode == DisplayMode.SinglePane || tab.IsLeftPaneActive)
        {
            tab.FilesActive = _fileSystemService.ListDirectory(tab.CurrentPath);
            tab.SelectedIndexActive = Math.Min(tab.SelectedIndexActive, Math.Max(0, tab.FilesActive.Count - 1));
        }
        else
        {
            tab.FilesPassive = _fileSystemService.ListDirectory(tab.PathPassive);
            tab.SelectedIndexPassive = Math.Min(tab.SelectedIndexPassive, Math.Max(0, tab.FilesPassive.Count - 1));
        }

        tab.IsDirty = false;
        TabStateChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Refreshes both panes (for dual pane mode)
    /// </summary>
    public void RefreshBothPanes()
    {
        var tab = ActiveTab;

        tab.FilesActive = _fileSystemService.ListDirectory(tab.CurrentPath);
        tab.SelectedIndexActive = Math.Min(tab.SelectedIndexActive, Math.Max(0, tab.FilesActive.Count - 1));

        if (tab.DisplayMode == DisplayMode.DualPane)
        {
            tab.FilesPassive = _fileSystemService.ListDirectory(tab.PathPassive);
            tab.SelectedIndexPassive = Math.Min(tab.SelectedIndexPassive, Math.Max(0, tab.FilesPassive.Count - 1));
        }

        tab.IsDirty = false;
        TabStateChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Navigates to a new path in the active pane
    /// </summary>
    public void NavigateTo(string path)
    {
        var tab = ActiveTab;
        var normalizedPath = _fileSystemService.NormalizePath(path);

        if (tab.DisplayMode == DisplayMode.SinglePane || tab.IsLeftPaneActive)
        {
            tab.CurrentPath = normalizedPath;
            tab.SelectedIndexActive = 0;
            tab.ScrollOffsetActive = 0;
        }
        else
        {
            tab.PathPassive = normalizedPath;
            tab.SelectedIndexPassive = 0;
            tab.ScrollOffsetPassive = 0;
        }

        RefreshActivePane();
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
}

