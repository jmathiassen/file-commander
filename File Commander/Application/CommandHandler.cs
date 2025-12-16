using File_Commander.Models;
using File_Commander.Services;

namespace File_Commander.Application;

/// <summary>
/// Handles user commands and orchestrates file operations
/// Phase 2: Implements staged operations and intelligent queue
/// </summary>
public class CommandHandler
{
    private readonly TabManager _tabManager;
    private readonly IntelligentTaskQueueService _taskQueue;
    private readonly ConfigService _configService;
    private readonly OperationBuffer _operationBuffer = new();

    public event EventHandler<string>? StatusMessage;
    public event EventHandler<(string Title, string Message, Action OnConfirm)>? ConfirmationRequired;
    public event EventHandler? OperationStarted;
    public event EventHandler? OperationCompleted;

    public CommandHandler(TabManager tabManager, IntelligentTaskQueueService taskQueue, ConfigService configService)
    {
        _tabManager = tabManager;
        _taskQueue = taskQueue;
        _configService = configService;

        // Subscribe to queue events for user feedback
        _taskQueue.JobStarted += (s, job) => StatusMessage?.Invoke(this, $"Started: {Path.GetFileName(job.SourcePath)}");
        _taskQueue.JobCompleted += (s, job) => StatusMessage?.Invoke(this, $"Completed: {Path.GetFileName(job.SourcePath)}");
        _taskQueue.JobFailed += (s, job) => StatusMessage?.Invoke(this, $"Failed: {job.ErrorMessage}");
    }

    /// <summary>
    /// Central command dispatcher - maps CommandFunction to handler methods
    /// Phase 2: Configurable keymap architecture
    /// </summary>
    public void ExecuteFunction(CommandFunction function)
    {
        switch (function)
        {
            // Navigation
            case CommandFunction.MOVE_CURSOR_UP:
                HandleMoveCursor(-1);
                break;
            case CommandFunction.MOVE_CURSOR_DOWN:
                HandleMoveCursor(1);
                break;
            case CommandFunction.MOVE_CURSOR_PAGE_UP:
                HandleMoveCursor(-10);
                break;
            case CommandFunction.MOVE_CURSOR_PAGE_DOWN:
                HandleMoveCursor(10);
                break;
            case CommandFunction.MOVE_CURSOR_HOME:
                HandleMoveCursorTo(0);
                break;
            case CommandFunction.MOVE_CURSOR_END:
                HandleMoveCursorTo(int.MaxValue);
                break;
            case CommandFunction.ENTER_DIRECTORY:
                HandleEnter();
                break;
            case CommandFunction.PARENT_DIRECTORY:
                HandleBackspace();
                break;

            // Pane Management
            case CommandFunction.SWITCH_PANE:
                _tabManager.SwitchActivePane();
                break;
            case CommandFunction.TOGGLE_DISPLAY_MODE:
                _tabManager.ToggleDisplayMode();
                break;

            // File Selection
            case CommandFunction.TOGGLE_MARK_STAY:
                ToggleFileMarkStay();
                break;
            case CommandFunction.TOGGLE_MARK_AND_MOVE:
                ToggleFileMarkAndMove();
                break;
            case CommandFunction.TOGGLE_MARK_AND_MOVE_UP:
                ToggleMarkWithMove(-1);
                break;
            case CommandFunction.TOGGLE_MARK_AND_MOVE_DOWN:
                ToggleMarkWithMove(1);
                break;
            case CommandFunction.MARK_ALL:
                HandleMarkAll();
                break;
            case CommandFunction.UNMARK_ALL:
                HandleUnmarkAll();
                break;

            // File Operations
            case CommandFunction.COPY_TO_OPPOSITE:
            case CommandFunction.STAGE_COPY:
                HandleCopy();  // Smart: immediate or staged based on mode
                break;
            case CommandFunction.MOVE_TO_OPPOSITE:
            case CommandFunction.STAGE_MOVE:
                HandleMove();  // Smart: immediate or staged based on mode
                break;
            case CommandFunction.EXECUTE_PASTE:
                HandlePaste();
                break;
            case CommandFunction.DELETE_FILES:
                HandleDelete();
                break;
            case CommandFunction.CREATE_DIRECTORY:
                // Requires dialog input - signal UI
                StatusMessage?.Invoke(this, "COMMAND:CREATE_DIRECTORY");
                break;

            // View Operations
            case CommandFunction.REFRESH_PANE:
                _tabManager.RefreshActivePane();
                StatusMessage?.Invoke(this, "Pane refreshed");
                break;
            case CommandFunction.REFRESH_BOTH_PANES:
                _tabManager.RefreshBothPanes();
                StatusMessage?.Invoke(this, "Both panes refreshed");
                break;

            // Tab Management
            case CommandFunction.CREATE_NEW_TAB:
                var tab = _tabManager.ActiveTab;
                var currentPath = tab.IsLeftPaneActive ? tab.CurrentPath : tab.PathPassive;
                _tabManager.CreateTab(currentPath);
                StatusMessage?.Invoke(this, "New tab created");
                break;
            case CommandFunction.CLOSE_CURRENT_TAB:
                _tabManager.CloseTab(_tabManager.ActiveTabIndex);
                StatusMessage?.Invoke(this, "Tab closed");
                break;
            case CommandFunction.SWITCH_TAB_NEXT:
                var nextIndex = (_tabManager.ActiveTabIndex + 1) % _tabManager.Tabs.Count;
                _tabManager.SwitchToTab(nextIndex);
                break;
            case CommandFunction.SWITCH_TAB_PREVIOUS:
                var prevIndex = (_tabManager.ActiveTabIndex - 1 + _tabManager.Tabs.Count) % _tabManager.Tabs.Count;
                _tabManager.SwitchToTab(prevIndex);
                break;
            case CommandFunction.SWITCH_TO_TAB_1:
            case CommandFunction.SWITCH_TO_TAB_2:
            case CommandFunction.SWITCH_TO_TAB_3:
            case CommandFunction.SWITCH_TO_TAB_4:
            case CommandFunction.SWITCH_TO_TAB_5:
            case CommandFunction.SWITCH_TO_TAB_6:
            case CommandFunction.SWITCH_TO_TAB_7:
            case CommandFunction.SWITCH_TO_TAB_8:
            case CommandFunction.SWITCH_TO_TAB_9:
                var tabIndex = function - CommandFunction.SWITCH_TO_TAB_1;
                if (tabIndex < _tabManager.Tabs.Count)
                {
                    _tabManager.SwitchToTab(tabIndex);
                }
                break;

            // Application
            case CommandFunction.QUIT_APPLICATION:
                StatusMessage?.Invoke(this, "COMMAND:QUIT");
                break;
            case CommandFunction.SHOW_HELP:
                StatusMessage?.Invoke(this, "COMMAND:SHOW_HELP");
                break;

            // Status Pane
            case CommandFunction.TOGGLE_STATUS_PANE_SIZE:
                StatusMessage?.Invoke(this, "COMMAND:TOGGLE_STATUS_PANE");
                break;

            // Options and Configuration
            case CommandFunction.SHOW_OPTIONS:
                StatusMessage?.Invoke(this, "COMMAND:SHOW_OPTIONS");
                break;
            case CommandFunction.CALCULATE_SIZE:
                HandleCalculateSize();
                break;

            // Diff/Sync Mode
            case CommandFunction.TOGGLE_DIFF_SYNC_MODE:
                HandleToggleDiffSyncMode();
                break;
            case CommandFunction.EXECUTE_SYNC:
                HandleExecuteSync();
                break;
            case CommandFunction.SWAP_DIFF_PANES:
                HandleSwapDiffPanes();
                break;

            case CommandFunction.NONE:
            case CommandFunction.UNKNOWN:
                // No action
                break;

            default:
                StatusMessage?.Invoke(this, $"Command not implemented: {function}");
                break;
        }
    }

    /// <summary>
    /// Moves cursor by delta in active pane
    /// </summary>
    private void HandleMoveCursor(int delta)
    {
        var tab = _tabManager.ActiveTab;
        var files = tab.IsLeftPaneActive ? tab.FilesActive : tab.FilesPassive;

        if (tab.IsLeftPaneActive)
        {
            var newIndex = tab.SelectedIndexActive + delta;
            tab.SelectedIndexActive = Math.Max(0, Math.Min(newIndex, files.Count - 1));
        }
        else
        {
            var newIndex = tab.SelectedIndexPassive + delta;
            tab.SelectedIndexPassive = Math.Max(0, Math.Min(newIndex, files.Count - 1));
        }

        _tabManager.NotifyStateChanged();
    }

    /// <summary>
    /// Moves cursor to absolute position in active pane
    /// </summary>
    private void HandleMoveCursorTo(int index)
    {
        var tab = _tabManager.ActiveTab;
        var files = tab.IsLeftPaneActive ? tab.FilesActive : tab.FilesPassive;

        var targetIndex = index == int.MaxValue ? files.Count - 1 : index;
        targetIndex = Math.Max(0, Math.Min(targetIndex, files.Count - 1));

        if (tab.IsLeftPaneActive)
        {
            tab.SelectedIndexActive = targetIndex;
        }
        else
        {
            tab.SelectedIndexPassive = targetIndex;
        }

        _tabManager.NotifyStateChanged();
    }

    /// <summary>
    /// Marks all files in active pane
    /// </summary>
    private void HandleMarkAll()
    {
        var tab = _tabManager.ActiveTab;
        var files = tab.IsLeftPaneActive ? tab.FilesActive : tab.FilesPassive;

        foreach (var file in files)
        {
            if (file.Name != "..")
            {
                tab.MarkedFiles.Add(file.FullPath);
            }
        }

        StatusMessage?.Invoke(this, $"Marked {tab.MarkedFiles.Count} files");
        _tabManager.NotifyStateChanged();
    }

    /// <summary>
    /// Unmarks all files
    /// </summary>
    private void HandleUnmarkAll()
    {
        var tab = _tabManager.ActiveTab;
        var count = tab.MarkedFiles.Count;
        tab.MarkedFiles.Clear();

        StatusMessage?.Invoke(this, $"Unmarked {count} files");
        _tabManager.NotifyStateChanged();
    }

    /// <summary>
    /// Handles Enter key - navigate into directory or open file
    /// </summary>
    public void HandleEnter()
    {
        var tab = _tabManager.ActiveTab;
        var files = tab.IsLeftPaneActive ? tab.FilesActive : tab.FilesPassive;
        var selectedIndex = tab.IsLeftPaneActive ? tab.SelectedIndexActive : tab.SelectedIndexPassive;

        if (selectedIndex >= 0 && selectedIndex < files.Count)
        {
            var selectedFile = files[selectedIndex];

            if (selectedFile.IsDirectory)
            {
                _tabManager.NavigateTo(selectedFile.FullPath);
            }
            else
            {
                StatusMessage?.Invoke(this, $"File: {selectedFile.Name} - Use F3 to view");
            }
        }
    }

    /// <summary>
    /// Handles Backspace - go to parent directory
    /// </summary>
    public void HandleBackspace()
    {
        var tab = _tabManager.ActiveTab;
        var currentPath = tab.IsLeftPaneActive ? tab.CurrentPath : tab.PathPassive;
        var parent = Directory.GetParent(currentPath);

        if (parent != null)
        {
            _tabManager.NavigateTo(parent.FullName);
        }
    }

    /// <summary>
    /// F5 - Copy (dual-pane: immediate to opposite, other: stage)
    /// </summary>
    public void HandleCopy()
    {
        var tab = _tabManager.ActiveTab;

        if (tab.DisplayMode != DisplayMode.DualPane)
        {
            // In single-pane mode, stage the operation
            HandleStageCopy();
            return;
        }

        // In dual-pane, perform immediate copy to opposite pane
        var destPath = tab.IsLeftPaneActive ? tab.PathPassive : tab.CurrentPath;
        var files = tab.IsLeftPaneActive ? tab.FilesActive : tab.FilesPassive;
        var selectedIndex = tab.IsLeftPaneActive ? tab.SelectedIndexActive : tab.SelectedIndexPassive;

        var filesToCopy = GetSelectedOrMarkedFiles(files, selectedIndex, tab.MarkedFiles);

        if (filesToCopy.Count == 0)
        {
            StatusMessage?.Invoke(this, "No files selected");
            return;
        }

        var message = $"Copy {filesToCopy.Count} item(s) to {destPath}?";
        ConfirmationRequired?.Invoke(this, ("Copy Files", message, async () =>
        {
            OperationStarted?.Invoke(this, EventArgs.Empty);

            try
            {
                await QueueFilesAsync(filesToCopy, destPath, OperationType.Copy);
                await Task.Delay(500); // Give jobs time to start
                _tabManager.RefreshBothPanes();
                tab.MarkedFiles.Clear();
            }
            finally
            {
                OperationCompleted?.Invoke(this, EventArgs.Empty);
            }
        }));
    }

    /// <summary>
    /// F6 - Move (dual-pane: immediate to opposite, other: stage)
    /// </summary>
    public void HandleMove()
    {
        var tab = _tabManager.ActiveTab;

        if (tab.DisplayMode != DisplayMode.DualPane)
        {
            // In single-pane mode, stage the operation
            HandleStageMove();
            return;
        }

        // In dual-pane, perform immediate move to opposite pane
        var destPath = tab.IsLeftPaneActive ? tab.PathPassive : tab.CurrentPath;
        var files = tab.IsLeftPaneActive ? tab.FilesActive : tab.FilesPassive;
        var selectedIndex = tab.IsLeftPaneActive ? tab.SelectedIndexActive : tab.SelectedIndexPassive;

        var filesToMove = GetSelectedOrMarkedFiles(files, selectedIndex, tab.MarkedFiles);

        if (filesToMove.Count == 0)
        {
            StatusMessage?.Invoke(this, "No files selected");
            return;
        }

        var message = $"Move {filesToMove.Count} item(s) to {destPath}?";
        ConfirmationRequired?.Invoke(this, ("Move Files", message, async () =>
        {
            OperationStarted?.Invoke(this, EventArgs.Empty);

            try
            {
                await QueueFilesAsync(filesToMove, destPath, OperationType.Move);
                await Task.Delay(500);
                _tabManager.RefreshBothPanes();
                tab.MarkedFiles.Clear();
            }
            finally
            {
                OperationCompleted?.Invoke(this, EventArgs.Empty);
            }
        }));
    }

    /// <summary>
    /// Stage Copy operation (for single-pane mode or manual staging)
    /// </summary>
    public void HandleStageCopy()
    {
        var tab = _tabManager.ActiveTab;
        var files = tab.IsLeftPaneActive ? tab.FilesActive : tab.FilesPassive;
        var selectedIndex = tab.IsLeftPaneActive ? tab.SelectedIndexActive : tab.SelectedIndexPassive;

        var filesToCopy = GetSelectedOrMarkedFiles(files, selectedIndex, tab.MarkedFiles);

        if (filesToCopy.Count == 0)
        {
            StatusMessage?.Invoke(this, "No files selected");
            return;
        }

        _operationBuffer.Operation = OperationType.Copy;
        _operationBuffer.SourcePaths = filesToCopy;

        StatusMessage?.Invoke(this, $"Staged {filesToCopy.Count} item(s) for COPY. Navigate and press Ctrl+V to paste.");
    }

    /// <summary>
    /// Stage Move operation (for single-pane mode or manual staging)
    /// </summary>
    public void HandleStageMove()
    {
        var tab = _tabManager.ActiveTab;
        var files = tab.IsLeftPaneActive ? tab.FilesActive : tab.FilesPassive;
        var selectedIndex = tab.IsLeftPaneActive ? tab.SelectedIndexActive : tab.SelectedIndexPassive;

        var filesToMove = GetSelectedOrMarkedFiles(files, selectedIndex, tab.MarkedFiles);

        if (filesToMove.Count == 0)
        {
            StatusMessage?.Invoke(this, "No files selected");
            return;
        }

        _operationBuffer.Operation = OperationType.Move;
        _operationBuffer.SourcePaths = filesToMove;

        StatusMessage?.Invoke(this, $"Staged {filesToMove.Count} item(s) for MOVE. Navigate and press Ctrl+V to paste.");
    }

    /// <summary>
    /// Ctrl+V - Execute staged operation (Paste)
    /// </summary>
    public void HandlePaste()
    {
        if (_operationBuffer.IsEmpty)
        {
            StatusMessage?.Invoke(this, "Nothing to paste. Use F5 (Copy) or F6 (Move) first.");
            return;
        }

        var tab = _tabManager.ActiveTab;
        var destPath = tab.IsLeftPaneActive ? tab.CurrentPath : tab.PathPassive;

        var operation = _operationBuffer.Operation;
        var sourcePaths = _operationBuffer.SourcePaths.ToList();

        var message = $"{operation} {sourcePaths.Count} item(s) to {destPath}?";
        ConfirmationRequired?.Invoke(this, ($"{operation} Files", message, async () =>
        {
            OperationStarted?.Invoke(this, EventArgs.Empty);

            try
            {
                await QueueFilesAsync(sourcePaths, destPath, operation);
                _operationBuffer.Clear();
                tab.MarkedFiles.Clear();

                await Task.Delay(500);
                _tabManager.RefreshBothPanes();

                StatusMessage?.Invoke(this, $"Queued {sourcePaths.Count} job(s) for {operation}");
            }
            finally
            {
                OperationCompleted?.Invoke(this, EventArgs.Empty);
            }
        }));
    }

    /// <summary>
    /// F7 - Create directory
    /// </summary>
    public void HandleMkDir(string dirName)
    {
        var tab = _tabManager.ActiveTab;
        var currentPath = tab.IsLeftPaneActive ? tab.CurrentPath : tab.PathPassive;

        try
        {
            var newDirPath = Path.Combine(currentPath, dirName);
            Directory.CreateDirectory(newDirPath);
            _tabManager.RefreshActivePane();
            StatusMessage?.Invoke(this, $"Created directory: {dirName}");
        }
        catch (Exception ex)
        {
            StatusMessage?.Invoke(this, $"Failed to create directory: {ex.Message}");
        }
    }

    /// <summary>
    /// F8 - Delete selected files
    /// </summary>
    public void HandleDelete()
    {
        var tab = _tabManager.ActiveTab;
        var files = tab.IsLeftPaneActive ? tab.FilesActive : tab.FilesPassive;
        var selectedIndex = tab.IsLeftPaneActive ? tab.SelectedIndexActive : tab.SelectedIndexPassive;

        var filesToDelete = GetSelectedOrMarkedFiles(files, selectedIndex, tab.MarkedFiles);

        if (filesToDelete.Count == 0)
        {
            StatusMessage?.Invoke(this, "No files selected");
            return;
        }

        var message = $"DELETE {filesToDelete.Count} item(s)? This cannot be undone!";
        ConfirmationRequired?.Invoke(this, ("Delete Files", message, async () =>
        {
            OperationStarted?.Invoke(this, EventArgs.Empty);

            try
            {
                // Delete operations don't have a destination
                foreach (var path in filesToDelete)
                {
                    var job = new FileOperationJob
                    {
                        Operation = OperationType.Delete,
                        SourcePath = path,
                        DestinationPath = string.Empty
                    };

                    await _taskQueue.EnqueueAsync(job);
                }

                await Task.Delay(500);
                _tabManager.RefreshActivePane();
                tab.MarkedFiles.Clear();
            }
            finally
            {
                OperationCompleted?.Invoke(this, EventArgs.Empty);
            }
        }));
    }

    /// <summary>
    /// Space - Toggle file mark WITHOUT moving cursor
    /// </summary>
    public void ToggleFileMarkStay()
    {
        ToggleFileMark();
    }

    /// <summary>
    /// Insert - Toggle file mark AND move down
    /// </summary>
    public void ToggleFileMarkAndMove()
    {
        var tab = _tabManager.ActiveTab;
        var files = tab.IsLeftPaneActive ? tab.FilesActive : tab.FilesPassive;

        ToggleFileMark();

        // Move to next file
        if (tab.IsLeftPaneActive)
        {
            tab.SelectedIndexActive = Math.Min(tab.SelectedIndexActive + 1, files.Count - 1);
        }
        else
        {
            tab.SelectedIndexPassive = Math.Min(tab.SelectedIndexPassive + 1, files.Count - 1);
        }
    }

    /// <summary>
    /// Shift+Up/Down - Toggle mark and move in direction (range selection)
    /// </summary>
    public void ToggleMarkWithMove(int direction)
    {
        var tab = _tabManager.ActiveTab;
        var files = tab.IsLeftPaneActive ? tab.FilesActive : tab.FilesPassive;

        ToggleFileMark();

        // Move in the specified direction
        if (tab.IsLeftPaneActive)
        {
            var newIndex = tab.SelectedIndexActive + direction;
            tab.SelectedIndexActive = Math.Max(0, Math.Min(newIndex, files.Count - 1));
        }
        else
        {
            var newIndex = tab.SelectedIndexPassive + direction;
            tab.SelectedIndexPassive = Math.Max(0, Math.Min(newIndex, files.Count - 1));
        }
    }

    /// <summary>
    /// Toggle file mark for batch operations (internal)
    /// </summary>
    private void ToggleFileMark()
    {
        var tab = _tabManager.ActiveTab;
        var files = tab.IsLeftPaneActive ? tab.FilesActive : tab.FilesPassive;
        var selectedIndex = tab.IsLeftPaneActive ? tab.SelectedIndexActive : tab.SelectedIndexPassive;

        if (selectedIndex >= 0 && selectedIndex < files.Count)
        {
            var selectedFile = files[selectedIndex];

            if (selectedFile.Name != "..")
            {
                if (tab.MarkedFiles.Contains(selectedFile.FullPath))
                {
                    tab.MarkedFiles.Remove(selectedFile.FullPath);
                }
                else
                {
                    tab.MarkedFiles.Add(selectedFile.FullPath);

                    // Calculate directory size if it's a directory and auto-calculate is enabled
                    if (selectedFile.IsDirectory && _configService.Settings.AutoCalculateDirectorySize)
                    {
                        _tabManager.CalculateDirectorySize(selectedFile);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Helper: Get marked files or current selection
    /// </summary>
    private List<string> GetSelectedOrMarkedFiles(List<FileItem> files, int selectedIndex, HashSet<string> markedFiles)
    {
        var result = new List<string>();

        if (markedFiles.Count > 0)
        {
            result.AddRange(markedFiles);
        }
        else if (selectedIndex >= 0 && selectedIndex < files.Count)
        {
            var selectedFile = files[selectedIndex];
            if (selectedFile.Name != "..")
            {
                result.Add(selectedFile.FullPath);
            }
        }

        return result;
    }

    /// <summary>
    /// Helper: Queue files as jobs with duplicate detection
    /// </summary>
    private async Task QueueFilesAsync(List<string> sourcePaths, string destPath, OperationType operation)
    {
        int successCount = 0;
        int skipCount = 0;

        // Pause queue if auto-start is disabled
        if (!_configService.Settings.AutoStartQueue && !_taskQueue.IsPaused)
        {
            _taskQueue.PauseQueue();
        }

        foreach (var sourcePath in sourcePaths)
        {
            var fileName = Path.GetFileName(sourcePath);
            var destFilePath = Path.Combine(destPath, fileName);

            var job = new FileOperationJob
            {
                Operation = operation,
                SourcePath = sourcePath,
                DestinationPath = destFilePath
            };

            try
            {
                await _taskQueue.EnqueueAsync(job);
                successCount++;
            }
            catch (InvalidOperationException ex)
            {
                // Job already queued or conflicts
                StatusMessage?.Invoke(this, $"Skipped: {ex.Message}");
                skipCount++;
            }
        }

        var summary = $"Queued {successCount} job(s)";
        if (skipCount > 0)
        {
            summary += $", skipped {skipCount} (already queued or conflicts)";
        }

        if (!_configService.Settings.AutoStartQueue && successCount > 0)
        {
            summary += " - Queue PAUSED, press Ctrl+R to start";
        }

        StatusMessage?.Invoke(this, summary);
    }

    /// <summary>
    /// Toggles diff/sync mode for the active tab
    /// </summary>
    private void HandleToggleDiffSyncMode()
    {
        var tab = _tabManager.ActiveTab;

        if (tab.DisplayMode == DisplayMode.DualPane_DiffSync)
        {
            // Switch back to dual pane
            tab.DisplayMode = DisplayMode.DualPane;
            StatusMessage?.Invoke(this, "Switched to dual pane mode");
        }
        else if (tab.DisplayMode == DisplayMode.DualPane)
        {
            // Switch to diff/sync mode
            tab.DisplayMode = DisplayMode.DualPane_DiffSync;
            StatusMessage?.Invoke(this, "Switched to diff/sync mode - F12 to execute sync");
        }
        else
        {
            StatusMessage?.Invoke(this, "Diff/sync mode requires dual pane mode (F9)");
        }
    }

    /// <summary>
    /// Swaps the diff/sync panes
    /// </summary>
    private void HandleSwapDiffPanes()
    {
        var tab = _tabManager.ActiveTab;

        if (tab.DisplayMode != DisplayMode.DualPane_DiffSync)
        {
            StatusMessage?.Invoke(this, "Swap only available in diff/sync mode");
            return;
        }

        // Swap paths
        var temp = tab.CurrentPath;
        tab.CurrentPath = tab.PathPassive;
        tab.PathPassive = temp;

        // Refresh both panes
        _tabManager.RefreshBothPanes();

        StatusMessage?.Invoke(this, "Swapped source and target paths");
    }

    /// <summary>
    /// Executes synchronization based on diff results
    /// </summary>
    private void HandleExecuteSync()
    {
        var tab = _tabManager.ActiveTab;

        if (tab.DisplayMode != DisplayMode.DualPane_DiffSync)
        {
            StatusMessage?.Invoke(this, "Sync only available in diff/sync mode (F11)");
            return;
        }

        var leftPath = tab.CurrentPath;
        var rightPath = tab.PathPassive;

        var message = $"Synchronize {leftPath} → {rightPath}?\n\nThis will copy newer/missing files from left to right.";
        ConfirmationRequired?.Invoke(this, ("Execute Sync", message, async () =>
        {
            OperationStarted?.Invoke(this, EventArgs.Empty);

            try
            {
                // Get diff results
                var diffService = new DirectoryDiffService();
                var diffResults = diffService.GetDirectoryDiff(leftPath, rightPath, isRecursive: true);

                var jobCount = 0;

                // Process each diff result
                foreach (var diff in diffResults)
                {
                    if (diff.RecommendedAction == SyncAction.CopyLeftToRight && diff.LeftFullPath != null)
                    {
                        var destPath = Path.Combine(rightPath, diff.RelativePath);

                        var job = new FileOperationJob
                        {
                            Operation = OperationType.Copy,
                            SourcePath = diff.LeftFullPath,
                            DestinationPath = destPath
                        };

                        await _taskQueue.EnqueueAsync(job);
                        jobCount++;
                    }
                    else if (diff.RecommendedAction == SyncAction.CopyRightToLeft && diff.RightFullPath != null)
                    {
                        var destPath = Path.Combine(leftPath, diff.RelativePath);

                        var job = new FileOperationJob
                        {
                            Operation = OperationType.Copy,
                            SourcePath = diff.RightFullPath,
                            DestinationPath = destPath
                        };

                        await _taskQueue.EnqueueAsync(job);
                        jobCount++;
                    }
                }

                StatusMessage?.Invoke(this, $"Queued {jobCount} sync job(s)");

                await Task.Delay(1000);
                _tabManager.RefreshBothPanes();
            }
            catch (Exception ex)
            {
                StatusMessage?.Invoke(this, $"Sync failed: {ex.Message}");
            }
            finally
            {
                OperationCompleted?.Invoke(this, EventArgs.Empty);
            }
        }));
    }

    /// <summary>
    /// Calculate directory size (F2 or triggered by marking with Space)
    /// </summary>
    private void HandleCalculateSize()
    {
        var tab = _tabManager.ActiveTab;
        var files = tab.IsLeftPaneActive ? tab.FilesActive : tab.FilesPassive;
        var selectedIndex = tab.IsLeftPaneActive ? tab.SelectedIndexActive : tab.SelectedIndexPassive;

        if (selectedIndex < 0 || selectedIndex >= files.Count)
        {
            StatusMessage?.Invoke(this, "No file selected");
            return;
        }

        var selectedFile = files[selectedIndex];

        if (!selectedFile.IsDirectory || selectedFile.Name == "..")
        {
            StatusMessage?.Invoke(this, "Select a directory to calculate size");
            return;
        }

        // Trigger async calculation via TabManager
        _tabManager.CalculateDirectorySize(selectedFile);
        StatusMessage?.Invoke(this, $"Calculating size for {selectedFile.Name}...");
    }
}

