using Terminal.Gui;
using File_Commander.Application;
using File_Commander.Models;
using File_Commander.Services;

namespace File_Commander.UI;

/// <summary>
/// Main window of the File Commander application
/// Phase 2: Configurable keymap architecture
/// Phase 3: Tabbed status pane
/// </summary>
public class MainWindow : Toplevel
{
    private readonly TabManager _tabManager;
    private readonly CommandHandler _commandHandler;
    private readonly KeymapService _keymapService;
    private readonly StatusPaneView _statusPane;
    private readonly ConfigService _configService;
    private readonly IntelligentTaskQueueService _taskQueue;

    private FilePaneView _leftPane = null!;
    private FilePaneView _rightPane = null!;
    private View _singlePaneContainer = null!;
    private View _dualPaneContainer = null!;
    private View _tabBar = null!;
    private List<Label> _tabLabels = new();
    private int _paneSplitPercent = 50; // Left pane percentage
    private int _statusPaneHeight = 6; // Track status pane height for layout

    // Single pane mode components
    private TreeView _treeView = null!;
    private TextView _previewPane = null!;
    private FilePaneView _singleFilePane = null!;
    public MainWindow(TabManager tabManager, CommandHandler commandHandler, KeymapService keymapService,
        StatusPaneView statusPane, ConfigService configService, IntelligentTaskQueueService taskQueue)
    {
        _tabManager = tabManager;
        _commandHandler = commandHandler;
        _keymapService = keymapService;
        _statusPane = statusPane;
        _configService = configService;
        _taskQueue = taskQueue;


        InitializeUI();
        SetupEventHandlers();

        // Initial refresh
        _tabManager.RefreshBothPanes();
        UpdateDisplay();
    }

    private void OnLeftPaneClicked()
    {
        var tab = _tabManager.ActiveTab;

        // Only switch if we're in dual-pane mode and right pane is currently active
        if (tab.DisplayMode == DisplayMode.DualPane && !tab.IsLeftPaneActive)
        {
            // Sync the current selected index from the right pane before switching
            tab.SelectedIndexPassive = _rightPane.GetSelectedIndex();

            // Switch to left pane
            _tabManager.SwitchActivePane();
        }

        // Always sync the selected index from the left pane
        tab.SelectedIndexActive = _leftPane.GetSelectedIndex();
        _tabManager.NotifyStateChanged();
    }

    private void OnRightPaneClicked()
    {
        var tab = _tabManager.ActiveTab;

        // Only switch if we're in dual-pane mode and left pane is currently active
        if (tab.DisplayMode == DisplayMode.DualPane && tab.IsLeftPaneActive)
        {
            // Sync the current selected index from the left pane before switching
            tab.SelectedIndexActive = _leftPane.GetSelectedIndex();

            // Switch to right pane
            _tabManager.SwitchActivePane();
        }

        // Always sync the selected index from the right pane
        tab.SelectedIndexPassive = _rightPane.GetSelectedIndex();
        _tabManager.NotifyStateChanged();
    }

    private void OnLeftPaneSelectionChanged()
    {
        var tab = _tabManager.ActiveTab;

        // Sync the selected index whenever cursor moves in left pane
        if (tab.DisplayMode == DisplayMode.SinglePane || tab.IsLeftPaneActive)
        {
            tab.SelectedIndexActive = _leftPane.GetSelectedIndex();
        }
    }

    private void OnRightPaneSelectionChanged()
    {
        var tab = _tabManager.ActiveTab;

        // Sync the selected index whenever cursor moves in right pane
        if (tab.DisplayMode == DisplayMode.DualPane && !tab.IsLeftPaneActive)
        {
            tab.SelectedIndexPassive = _rightPane.GetSelectedIndex();
        }
    }

    public override void LayoutSubviews()
    {
        base.LayoutSubviews();

        // Fixed status pane at 20 lines
        const int statusHeight = 20;
        var newContainerHeight = Dim.Fill() - (1 + statusHeight); // tab (1) + status pane (20)

        _dualPaneContainer.Height = newContainerHeight;
        _singlePaneContainer.Height = newContainerHeight;

        // Keep status pane at bottom
        _statusPane.Y = Pos.AnchorEnd(statusHeight);
    }

    private void InitializeUI()
    {
        // Tab bar at top
        _tabBar = new View
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = 1,
            ColorScheme = Colors.Menu
        };

        // Single pane container
        _singlePaneContainer = new View
        {
            X = 0,
            Y = 1,  // Below tab bar
            Width = Dim.Fill(),
            Height = Dim.Fill() - 21  // Leave room for tab bar (1) and status pane (20)
        };

        _singlePaneContainer.Add(_treeView, _singleFilePane, _previewPane);
        _dualPaneContainer = new View
        {
            X = 0,
            Y = 1,  // Below tab bar
            Width = Dim.Fill(),
            Height = Dim.Fill() - 7  // Leave room for tab bar (1) and status pane (6)
        };

        // Left pane
        _leftPane = new FilePaneView("Left Pane")
        {
            X = 0,
            Y = 0,
            Width = Dim.Percent(50),
            Height = Dim.Fill()
        };

        _leftPane.FileActivated += (s, file) => _commandHandler.HandleEnter();
        _leftPane.PaneClicked += (s, e) => OnLeftPaneClicked();
        _leftPane.FileSelected += (s, file) => OnLeftPaneSelectionChanged();

        // Right pane - use Dim.Fill() to ensure it reaches the edge
        _rightPane = new FilePaneView("Right Pane")
        {
            X = Pos.Percent(50),
            Y = 0,
            Width = Dim.Fill(), // Use Fill instead of Percent to reach edge
            Height = Dim.Fill()
        };

        _rightPane.FileActivated += (s, file) => _commandHandler.HandleEnter();
        _rightPane.PaneClicked += (s, e) => OnRightPaneClicked();
        _rightPane.FileSelected += (s, file) => OnRightPaneSelectionChanged();

        _dualPaneContainer.Add(_leftPane, _rightPane);

        // Single pane mode components
        // Tree view on the left (20% width)
        _treeView = new TreeView
        {
            X = 0,
            Y = 0,
            Width = Dim.Percent(20),
            Height = Dim.Fill()
        };

        // File pane in the center (50% width)
        _singleFilePane = new FilePaneView("Files")
        {
            X = Pos.Percent(20),
            Y = 0,
            Width = Dim.Percent(50),
            Height = Dim.Fill()
        };

        _singleFilePane.FileActivated += (s, file) => _commandHandler.HandleEnter();

        // Preview pane on the right (30% width)
        _previewPane = new TextView
        {
            X = Pos.Percent(70),
            Y = 0,
            Width = Dim.Percent(30),
            Height = Dim.Fill(),
            ReadOnly = true,
            Text = "Preview\n\nSelect a file to view its contents"
        };

        _singlePaneContainer.Add(_treeView, _singleFilePane, _previewPane);

        // Status pane - fixed at 20 lines
        _statusPane.Y = Pos.AnchorEnd(20);
        _statusPane.Height = 20;

        // Initialize tab labels
        UpdateTabBar();

        Add(_tabBar, _dualPaneContainer, _statusPane);
    }

    private void SetupEventHandlers()
    {
        _tabManager.TabChanged += (s, e) => UpdateTabBar();
        _tabManager.TabStateChanged += (s, e) => UpdateDisplay();
        _tabManager.DirectoryRefreshed += (s, path) =>
            _statusPane.LogActivity($"Directory refreshed: {path}");

        _commandHandler.StatusMessage += (s, msg) =>
        {
            _statusPane.AddCommandHistory(msg);

            // Update info tab with current state including directory size
            var tab = _tabManager.ActiveTab;
            var markedCount = tab.MarkedFiles.Count.ToString();
            var activeDir = tab.IsLeftPaneActive ? tab.CurrentPath : tab.PathPassive;

            // Calculate total size of marked files or active directory
            long? totalSize = null;
            if (tab.MarkedFiles.Count > 0)
            {
                // Sum up marked files
                long sum = 0;
                var files = tab.IsLeftPaneActive ? tab.FilesActive : tab.FilesPassive;
                foreach (var file in files)
                {
                    if (tab.MarkedFiles.Contains(file.FullPath))
                    {
                        if (file.CalculatedSize.HasValue)
                        {
                            sum += file.CalculatedSize.Value;
                        }
                        else if (!file.IsDirectory)
                        {
                            sum += file.Size;
                        }
                    }
                }
                totalSize = sum;
            }
            else
            {
                // Try to get current directory size
                var files = tab.IsLeftPaneActive ? tab.FilesActive : tab.FilesPassive;
                var currentDirItem = files.FirstOrDefault(f => f.FullPath == activeDir);
                if (currentDirItem?.CalculatedSize != null)
                {
                    totalSize = currentDirItem.CalculatedSize;
                }
            }

            _statusPane.UpdateInfo(markedCount, activeDir, totalSize);
        };

        _commandHandler.ConfirmationRequired += (s, args) =>
        {
            var (title, message, onConfirm) = args;
            var result = MessageBox.Query(title, message, "Yes", "No");

            if (result == 0)
            {
                onConfirm?.Invoke();
            }
        };

        // Key bindings - Phase 2: Configurable keymap architecture
        KeyPress += (e) =>
        {
            var key = e.KeyEvent.Key;

            // Resolve key to command function
            var function = _keymapService.Resolve(key);

            if (function == CommandFunction.UNKNOWN || function == CommandFunction.NONE)
            {
                // Let default handlers process unknown keys
                return;
            }

            // Handle special commands that require UI interaction
            if (function == CommandFunction.CREATE_DIRECTORY)
            {
                PromptForMkDir();
                e.Handled = true;
                return;
            }

            if (function == CommandFunction.QUIT_APPLICATION)
            {
                e.Handled = true;
                Terminal.Gui.Application.RequestStop();
                return;
            }

            if (function == CommandFunction.SHOW_OPTIONS)
            {
                ShowOptionsDialog();
                e.Handled = true;
                return;
            }

            if (function == CommandFunction.TOGGLE_STATUS_PANE_SIZE)
            {
                // Status pane is fixed at 20 lines for now
                // This command is disabled
                e.Handled = true;
                return;
            }

            if (function == CommandFunction.SWITCH_STATUS_TAB)
            {
                _statusPane.SwitchToNextTab();
                e.Handled = true;
                return;
            }

            if (function == CommandFunction.INCREASE_LEFT_PANE)
            {
                AdjustPaneSplit(5);
                e.Handled = true;
                return;
            }

            if (function == CommandFunction.DECREASE_LEFT_PANE)
            {
                AdjustPaneSplit(-5);
                e.Handled = true;
                return;
            }

            if (function == CommandFunction.RESET_PANE_SPLIT)
            {
                _paneSplitPercent = 50;
                UpdatePaneSplit();
                e.Handled = true;
                return;
            }
            // Execute command through handler
            _commandHandler.ExecuteFunction(function);

            // Update display after most commands
            if (ShouldUpdateDisplayAfter(function))
            {
                UpdateDisplay();
            }

            e.Handled = true;
        };
    }

    /// <summary>
    /// Determines if display should update after a command
    /// </summary>
    private bool ShouldUpdateDisplayAfter(CommandFunction function)
    {
        return function switch
        {
            CommandFunction.MOVE_CURSOR_UP => true,
            CommandFunction.MOVE_CURSOR_DOWN => true,
            CommandFunction.MOVE_CURSOR_PAGE_UP => true,
            CommandFunction.MOVE_CURSOR_PAGE_DOWN => true,
            CommandFunction.MOVE_CURSOR_HOME => true,
            CommandFunction.MOVE_CURSOR_END => true,
            CommandFunction.SWITCH_PANE => true,
            CommandFunction.TOGGLE_DISPLAY_MODE => true,
            CommandFunction.TOGGLE_MARK_STAY => true,
            CommandFunction.TOGGLE_MARK_AND_MOVE => true,
            CommandFunction.TOGGLE_MARK_AND_MOVE_UP => true,
            CommandFunction.TOGGLE_MARK_AND_MOVE_DOWN => true,
            CommandFunction.MARK_ALL => true,
            CommandFunction.UNMARK_ALL => true,
            CommandFunction.REFRESH_PANE => true,
            CommandFunction.REFRESH_BOTH_PANES => true,
            CommandFunction.CREATE_NEW_TAB => true,
            CommandFunction.CLOSE_CURRENT_TAB => true,
            CommandFunction.SWITCH_TAB_NEXT => true,
            CommandFunction.SWITCH_TAB_PREVIOUS => true,
            CommandFunction.TOGGLE_DIFF_SYNC_MODE => true,
            CommandFunction.SWAP_DIFF_PANES => true,
            _ when function >= CommandFunction.SWITCH_TO_TAB_1 && function <= CommandFunction.SWITCH_TO_TAB_9 => true,
            _ => false
        };
    }

    private void UpdateDisplay()
    {
        var tab = _tabManager.ActiveTab;

        // Update pane visibility based on mode
        if (tab.DisplayMode == DisplayMode.DualPane)
        {
            Remove(_singlePaneContainer);
            if (!Subviews.Contains(_dualPaneContainer))
            {
                Add(_dualPaneContainer);
            }

            // Update both panes - normal dual pane mode
            _leftPane.Title = $"Left: {tab.CurrentPath}";
            _leftPane.SetCurrentPath(tab.CurrentPath);
            _leftPane.SetFiles(tab.FilesActive, tab.MarkedFiles, tab.SelectedIndexActive,
                _configService.Settings.ShowFileIcons, _configService.Settings.UseNarrowIcons,
                _configService.Settings.ShowSecondsInDate, _configService.Settings.ShowExtensionsInColumn,
                _taskQueue, _configService.Settings.FileSizeFormat);
            _leftPane.SetActive(tab.IsLeftPaneActive);

            _rightPane.Title = $"Right: {tab.PathPassive}";
            _rightPane.SetCurrentPath(tab.PathPassive);
            _rightPane.SetFiles(tab.FilesPassive, tab.MarkedFiles, tab.SelectedIndexPassive,
                _configService.Settings.ShowFileIcons, _configService.Settings.UseNarrowIcons,
                _configService.Settings.ShowSecondsInDate, _configService.Settings.ShowExtensionsInColumn,
                _taskQueue, _configService.Settings.FileSizeFormat);
            _rightPane.SetActive(!tab.IsLeftPaneActive);

            // CRITICAL FIX: Explicitly set focus on the active pane
            if (tab.IsLeftPaneActive)
            {
                _leftPane.SetFocus();
            }
            else
            {
                _rightPane.SetFocus();
            }
        }
        else if (tab.DisplayMode == DisplayMode.DualPane_DiffSync)
        {
            // Diff/Sync mode
            Remove(_singlePaneContainer);
            if (!Subviews.Contains(_dualPaneContainer))
            {
                Add(_dualPaneContainer);
            }

            // Update pane titles to reflect diff/sync mode
            _leftPane.Title = $"Source: {tab.CurrentPath} [Diff/Sync]";
            _rightPane.Title = $"Target: {tab.PathPassive} [Diff/Sync]";

            // Get diff results and display them
            UpdateDiffSyncDisplay(tab);

            // Set focus on active pane
            if (tab.IsLeftPaneActive)
            {
                _leftPane.SetFocus();
            }
            else
            {
                _rightPane.SetFocus();
            }
        }
        else
        {
            Remove(_dualPaneContainer);
            if (!Subviews.Contains(_singlePaneContainer))
            {
                Add(_singlePaneContainer);
            }

            // Single pane mode - populate tree view and file pane
            UpdateSinglePaneMode();
        }

        SetNeedsDisplay();
    }

    /// <summary>
    /// Updates single pane mode UI components
    /// </summary>
    private void UpdateSinglePaneMode()
    {
        var tab = _tabManager.ActiveTab;

        // Update file pane
        _singleFilePane.Title = $"Files: {tab.CurrentPath}";
        _singleFilePane.SetCurrentPath(tab.CurrentPath);
        _singleFilePane.SetFiles(tab.FilesActive, tab.MarkedFiles, tab.SelectedIndexActive,
            _configService.Settings.ShowFileIcons, _configService.Settings.UseNarrowIcons,
            _configService.Settings.ShowSecondsInDate, _configService.Settings.ShowExtensionsInColumn,
            _taskQueue, _configService.Settings.FileSizeFormat);
        _singleFilePane.SetActive(true);
        _singleFilePane.SetFocus(); // Ensure the FilePane has focus for navigation

        // Update tree view with directory structure
        UpdateTreeView(tab.CurrentPath);

        // Update preview pane if a file is selected
        UpdatePreviewPane();

        _statusPane.AddCommandHistory($"Single pane mode: {tab.CurrentPath}");
    }

    /// <summary>
    /// Updates the tree view with directory structure
    /// </summary>
    private void UpdateTreeView(string currentPath)
    {
        try
        {
            var rootDir = new DirectoryInfo(currentPath);

            // Create a simple tree structure using strings
            var treeItems = new List<string>();
            treeItems.Add($"📁 {rootDir.Name}");

            // Add subdirectories
            try
            {
                foreach (var dir in rootDir.GetDirectories())
                {
                    treeItems.Add($"  📁 {dir.Name}");
                }
            }
            catch
            {
                // Ignore permission errors
            }

            // Terminal.Gui TreeView expects ITreeNode objects
            // For now, use a simple label-based display
            _treeView.ClearObjects();

            // Add parent directory as root
            var parentPath = Directory.GetParent(currentPath);
            if (parentPath != null)
            {
                _treeView.AddObject(new TreeViewItem("📁 ..", parentPath.FullName));
            }

            // Add current directory
            _treeView.AddObject(new TreeViewItem($"📁 {rootDir.Name}", rootDir.FullName));

            // Add subdirectories
            foreach (var dir in rootDir.GetDirectories())
            {
                try
                {
                    _treeView.AddObject(new TreeViewItem($"📁 {dir.Name}", dir.FullName));
                }
                catch
                {
                    // Ignore permission errors
                }
            }
        }
        catch
        {
            // Fallback if tree view fails
            _treeView.ClearObjects();
        }
    }

    /// <summary>
    /// Simple tree view item for directory navigation
    /// </summary>
    private class TreeViewItem : Terminal.Gui.Trees.ITreeNode
    {
        public string DisplayName { get; }
        public string FullPath { get; }

        public TreeViewItem(string displayName, string fullPath)
        {
            DisplayName = displayName;
            FullPath = fullPath;
            Tag = fullPath; // Initialize Tag for ITreeNode
            Text = displayName; // Initialize Text for ITreeNode
        }

        public override string ToString() => DisplayName;

        // ITreeNode implementation
        public string Text { get; set; }
        public IList<Terminal.Gui.Trees.ITreeNode> Children => new List<Terminal.Gui.Trees.ITreeNode>();
        public Terminal.Gui.Trees.ITreeNode? Parent { get; set; }
        public object Tag { get; set; }
    }

    /// <summary>
    /// Updates the preview pane with selected file content
    /// </summary>
    private void UpdatePreviewPane()
    {
        var tab = _tabManager.ActiveTab;
        var files = tab.FilesActive;
        var selectedIndex = tab.SelectedIndexActive;

        if (selectedIndex >= 0 && selectedIndex < files.Count)
        {
            var selectedFile = files[selectedIndex];

            if (!selectedFile.IsDirectory && selectedFile.Name != "..")
            {
                try
                {
                    // Try to read and preview the file (first 1000 lines)
                    var lines = File.ReadLines(selectedFile.FullPath).Take(1000).ToList();
                    _previewPane.Text = string.Join("\n", lines);
                }
                catch
                {
                    _previewPane.Text = $"[Cannot preview file]\n\nFile: {selectedFile.Name}\nSize: {selectedFile.FormattedSize}\nModified: {selectedFile.FormattedDate}";
                }
            }
            else
            {
                _previewPane.Text = $"[Directory]\n\n{selectedFile.Name}\n\nPress Enter to navigate";
            }
        }
        else
        {
            _previewPane.Text = "Preview\n\nSelect a file to view its contents";
        }
    }

    /// <summary>
    /// Updates display for diff/sync mode
    /// </summary>
    private void UpdateDiffSyncDisplay(TabState tab)
    {
        try
        {
            var diffService = new DirectoryDiffService();
            var diffResults = diffService.GetDirectoryDiff(tab.CurrentPath, tab.PathPassive, isRecursive: false);

            // Create enhanced file items with diff status
            var leftFiles = new List<FileItem>();
            var rightFiles = new List<FileItem>();

            foreach (var diff in diffResults)
            {
                var diffIndicator = diff.DiffType switch
                {
                    DiffType.Identical => "=",
                    DiffType.LeftOnly => "→",
                    DiffType.RightOnly => "←",
                    DiffType.LeftNewer => "»",
                    DiffType.RightNewer => "«",
                    DiffType.Conflict => "!",
                    _ => " "
                };

                // Add to left pane if exists on left
                if (diff.LeftFullPath != null)
                {
                    leftFiles.Add(new FileItem
                    {
                        Name = $"{diffIndicator} {diff.RelativePath}",
                        FullPath = diff.LeftFullPath,
                        Size = diff.LeftSize ?? 0,
                        IsDirectory = diff.IsDirectory,
                        LastModified = diff.LeftModified ?? DateTime.MinValue
                    });
                }
                else
                {
                    // Placeholder for right-only files
                    leftFiles.Add(new FileItem
                    {
                        Name = $"{diffIndicator} {diff.RelativePath}",
                        FullPath = "",
                        Size = 0,
                        IsDirectory = diff.IsDirectory,
                        LastModified = DateTime.MinValue
                    });
                }

                // Add to right pane if exists on right
                if (diff.RightFullPath != null)
                {
                    rightFiles.Add(new FileItem
                    {
                        Name = $"{diffIndicator} {diff.RelativePath}",
                        FullPath = diff.RightFullPath,
                        Size = diff.RightSize ?? 0,
                        IsDirectory = diff.IsDirectory,
                        LastModified = diff.RightModified ?? DateTime.MinValue
                    });
                }
                else
                {
                    // Placeholder for left-only files
                    rightFiles.Add(new FileItem
                    {
                        Name = $"{diffIndicator} {diff.RelativePath}",
                        FullPath = "",
                        Size = 0,
                        IsDirectory = diff.IsDirectory,
                        LastModified = DateTime.MinValue
                    });
                }
            }

            // Update panes with diff results
            _leftPane.SetFiles(leftFiles, new HashSet<string>(), tab.SelectedIndexActive,
                _configService.Settings.ShowFileIcons, _configService.Settings.UseNarrowIcons,
                _configService.Settings.ShowSecondsInDate, _configService.Settings.ShowExtensionsInColumn,
                _taskQueue, _configService.Settings.FileSizeFormat);
            _rightPane.SetFiles(rightFiles, new HashSet<string>(), tab.SelectedIndexPassive,
                _configService.Settings.ShowFileIcons, _configService.Settings.UseNarrowIcons,
                _configService.Settings.ShowSecondsInDate, _configService.Settings.ShowExtensionsInColumn,
                _taskQueue, _configService.Settings.FileSizeFormat);

            _statusPane.AddCommandHistory($"Diff: {diffResults.Count} items compared - = identical, → left only, ← right only, » left newer, « right newer");
        }
        catch (Exception ex)
        {
            _statusPane.AddCommandHistory($"Diff failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates the tab bar with current tabs
    /// </summary>
    private void UpdateTabBar()
    {
        // Clear existing labels
        _tabBar.RemoveAll();
        _tabLabels.Clear();

        var x = 0;
        var tabs = _tabManager.Tabs;
        var activeIndex = _tabManager.ActiveTabIndex;

        for (int i = 0; i < tabs.Count; i++)
        {
            var tab = tabs[i];
            var tabPath = Path.GetFileName(tab.CurrentPath);
            if (string.IsNullOrEmpty(tabPath))
            {
                tabPath = tab.CurrentPath;
            }

            var tabIndex = i; // Capture for closure
            var label = new Label
            {
                X = x,
                Y = 0,
                Text = $" [{i + 1}] {tabPath} ",
                ColorScheme = i == activeIndex ? Colors.Base : Colors.TopLevel
            };

            // Add mouse click handler for tab switching
            label.MouseClick += (args) =>
            {
                if (tabIndex < _tabManager.Tabs.Count)
                {
                    _commandHandler.ExecuteFunction(CommandFunction.SWITCH_TO_TAB_1 + tabIndex);
                    UpdateDisplay();
                    UpdateTabBar();
                }
                args.Handled = true;
            };

            _tabLabels.Add(label);
            _tabBar.Add(label);

            x += label.Text.Length + 1;
        }

        // Add help text at the end showing key bindings for main operations
        var quitKeys = _configService.Settings.KeyBindings.TryGetValue("Quit", out var quitBinding)
            ? string.Join("/", quitBinding.Keys)
            : "F10";
        var optionsKeys = _configService.Settings.KeyBindings.TryGetValue("Options", out var optionsBinding)
            ? string.Join("/", optionsBinding.Keys)
            : "Ctrl+O";

        var helpText = new Label
        {
            X = Pos.AnchorEnd(50),
            Y = 0,
            Text = $"{optionsKeys}:Options {quitKeys}:Quit",
            ColorScheme = Colors.Menu
        };

        _tabBar.Add(helpText);
    }

    private void PromptForMkDir()
    {
        var dialog = new Dialog("Create Directory", 60, 10);

        var label = new Label("Directory name:")
        {
            X = 1,
            Y = 1
        };

        var textField = new TextField("")
        {
            X = 1,
            Y = 2,
            Width = Dim.Fill() - 2
        };

        var btnOk = new Button("OK")
        {
            X = Pos.Center() - 10,
            Y = Pos.Bottom(textField) + 1
        };

        btnOk.Clicked += () =>
        {
            var dirName = textField.Text.ToString();
            if (!string.IsNullOrWhiteSpace(dirName))
            {
                _commandHandler.HandleMkDir(dirName);
                dialog.Running = false;
            }
        };

        var btnCancel = new Button("Cancel")
        {
            X = Pos.Center() + 2,
            Y = Pos.Bottom(textField) + 1
        };

        btnCancel.Clicked += () => dialog.Running = false;

        dialog.Add(label, textField, btnOk, btnCancel);

        Terminal.Gui.Application.Run(dialog);
    }

    private void ShowOptionsDialog()
    {
        var dialog = new OptionsDialog(_configService.Settings);
        Terminal.Gui.Application.Run(dialog);

        if (dialog.WasSaved)
        {
            _configService.UpdateSettings(dialog.GetSettings());
            _statusPane.AddCommandHistory($"Options saved - Update mode: {dialog.GetSettings().DirectoryUpdateMode}");
            UpdateDisplay(); // Refresh to apply new settings
        }
    }

    /// <summary>
    /// Adjusts the pane split by the specified delta
    /// </summary>
    private void AdjustPaneSplit(int delta)
    {
        _paneSplitPercent = Math.Clamp(_paneSplitPercent + delta, 10, 90);
        UpdatePaneSplit();
    }

    /// <summary>
    /// Updates the pane widths based on current split percentage
    /// </summary>
    private void UpdatePaneSplit()
    {
        _leftPane.Width = Dim.Percent(_paneSplitPercent);
        _rightPane.X = Pos.Percent(_paneSplitPercent);
        _rightPane.Width = Dim.Percent(100 - _paneSplitPercent);
        SetNeedsDisplay();
    }
}

