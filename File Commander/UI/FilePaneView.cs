using Terminal.Gui;
using File_Commander.Models;

namespace File_Commander.UI;

/// <summary>
/// View for displaying a file list pane
/// </summary>
public class FilePaneView : FrameView
{
    private readonly ListView _listView;
    private readonly Label _statusBar;
    private List<FileItem> _files = new();
    private HashSet<string> _markedFiles = new();
    private bool _isActive = true;
    private string _currentPath = "";

    public event EventHandler<FileItem>? FileSelected;
    public event EventHandler<FileItem>? FileActivated;

    public FilePaneView(string title) : base(title)
    {
        _listView = new ListView
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill() - 1, // Leave space for status bar
            AllowsMarking = false,
            AllowsMultipleSelection = false
        };

        _listView.SelectedItemChanged += OnSelectedItemChanged;
        _listView.OpenSelectedItem += OnOpenSelectedItem;

        _statusBar = new Label
        {
            X = 0,
            Y = Pos.AnchorEnd(1),
            Width = Dim.Fill(),
            Height = 1,
            Text = ""
        };

        Add(_listView);
        Add(_statusBar);

        // Setup custom row rendering for directory colors
        SetupCustomRendering();
    }

    private void SetupCustomRendering()
    {
        _listView.RowRender += (args) =>
        {
            if (args.Row < 0 || args.Row >= _files.Count)
                return;

            var file = _files[args.Row];

            // Don't override selection colors - Terminal.Gui handles this
            // Just set the text color for directories when NOT selected
            if (file.IsDirectory && file.Name != ".." && args.Row != _listView.SelectedItem)
            {
                // Use BrightCyan for directories, keep default background
                args.RowAttribute = new Terminal.Gui.Attribute(Color.BrightCyan, Color.Black);
            }
            // For selected items, Terminal.Gui will use the focus/selection colors automatically
        };
    }

    private void OnSelectedItemChanged(ListViewItemEventArgs e)
    {
        if (e.Item >= 0 && e.Item < _files.Count)
        {
            FileSelected?.Invoke(this, _files[e.Item]);
        }
    }

    private void OnOpenSelectedItem(ListViewItemEventArgs e)
    {
        if (_listView.SelectedItem >= 0 && _listView.SelectedItem < _files.Count)
        {
            FileActivated?.Invoke(this, _files[_listView.SelectedItem]);
        }
    }

    public void SetFiles(List<FileItem> files, HashSet<string> markedFiles, int selectedIndex = 0,
        bool showIcons = false, bool useNarrowIcons = true, bool showSeconds = true, bool showExtensionColumn = false)
    {
        _files = files;
        _markedFiles = markedFiles;

        // Calculate available width for filename
        var dateWidth = showSeconds ? 19 : 16; // "yyyy-MM-dd HH:mm:ss" or "yyyy-MM-dd HH:mm"
        var sizeWidth = 12; // Right-aligned size column
        var markWidth = 1; // "*" or " " - no space after mark
        var iconWidth = showIcons ? 2 : 0; // "D " or "F " only if enabled
        var extWidth = showExtensionColumn ? 8 : 0; // Extension column width

        // Calculate separators: vertical bars and spaces
        // Format: "name │ size │ date" = 3 bars × 3 chars each (" │ ") = 9
        // or: "name │ ext │ size │ date" = 4 bars × 3 chars = 12
        var separators = showExtensionColumn ? 12 : 9;

        var availableWidth = Bounds.Width > 0 ? Bounds.Width : 80;
        var maxNameWidth = availableWidth - dateWidth - sizeWidth - markWidth - iconWidth - extWidth - separators;
        if (maxNameWidth < 10) maxNameWidth = 10; // Minimum filename width

        var displayItems = files.Select(f =>
        {
            var mark = markedFiles.Contains(f.FullPath) ? "*" : " ";
            var icon = showIcons
                ? (useNarrowIcons
                    ? (f.IsDirectory ? "D " : "F ")
                    : (f.IsDirectory ? "📁 " : "📄 "))
                : "";

            string name;
            string ext = "";

            if (showExtensionColumn && !f.IsDirectory && f.Name != "..")
            {
                // Split filename and extension
                var dotIndex = f.Name.LastIndexOf('.');
                if (dotIndex > 0)
                {
                    name = f.Name.Substring(0, dotIndex);
                    ext = f.Name.Substring(dotIndex); // Include the dot
                }
                else
                {
                    name = f.Name;
                    ext = "";
                }
            }
            else
            {
                name = f.Name;
            }

            // Truncate filename if needed
            if (name.Length > maxNameWidth)
            {
                name = name.Substring(0, maxNameWidth - 3) + "...";
            }

            // Build the display string with vertical bars between columns
            var namePadded = name.PadRight(maxNameWidth);
            var extPadded = showExtensionColumn ? ext.PadRight(extWidth) : "";
            var sizePadded = f.FormattedSize.PadLeft(sizeWidth);
            var datePadded = f.GetFormattedDate(showSeconds);

            return showExtensionColumn
                ? $"{mark}{icon}{namePadded} │ {extPadded} │ {sizePadded} │ {datePadded}"
                : $"{mark}{icon}{namePadded} │ {sizePadded} │ {datePadded}";
        }).ToList();

        _listView.SetSource(displayItems);

        // CRITICAL FIX: Always synchronize selection index with TabState
        _listView.SelectedItem = Math.Min(selectedIndex, Math.Max(0, displayItems.Count - 1));

        // Update status bar
        UpdateStatusBar();
    }

    public void SetActive(bool isActive)
    {
        _isActive = isActive;
        ColorScheme = isActive ? Colors.Base : Colors.TopLevel;
    }

    /// <summary>
    /// Updates the status bar with disk space, file counts, and selection info
    /// </summary>
    private void UpdateStatusBar()
    {
        try
        {
            // Get disk info
            var diskInfo = "";
            if (!string.IsNullOrEmpty(_currentPath) && Directory.Exists(_currentPath))
            {
                try
                {
                    var drive = new DriveInfo(Path.GetPathRoot(_currentPath) ?? "");
                    var freeSpace = FormatBytes(drive.AvailableFreeSpace);
                    var totalSpace = FormatBytes(drive.TotalSize);
                    diskInfo = $"{freeSpace} free of {totalSpace}";
                }
                catch
                {
                    diskInfo = "Disk info unavailable";
                }
            }

            // Count files and directories (excluding ..)
            var fileCount = _files.Count(f => !f.IsDirectory && f.Name != "..");
            var dirCount = _files.Count(f => f.IsDirectory && f.Name != "..");
            var itemsInfo = $"{fileCount} file{(fileCount != 1 ? "s" : "")}, {dirCount} dir{(dirCount != 1 ? "s" : "")}";

            // Calculate marked items info
            var markedInfo = "";
            if (_markedFiles.Count > 0)
            {
                long markedSize = 0;
                foreach (var file in _files.Where(f => _markedFiles.Contains(f.FullPath)))
                {
                    if (file.CalculatedSize.HasValue)
                        markedSize += file.CalculatedSize.Value;
                    else if (!file.IsDirectory)
                        markedSize += file.Size;
                }
                markedInfo = $" | {_markedFiles.Count} selected ({FormatBytes(markedSize)})";
            }

            _statusBar.Text = $" {diskInfo} | {itemsInfo}{markedInfo}";
        }
        catch
        {
            _statusBar.Text = " Status unavailable";
        }
    }

    /// <summary>
    /// Sets the current path for disk info display
    /// </summary>
    public void SetCurrentPath(string path)
    {
        _currentPath = path;
        UpdateStatusBar();
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double size = bytes;

        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size = size / 1024;
        }

        return $"{size:0.##} {sizes[order]}";
    }

    public int SelectedIndex => _listView.SelectedItem;

    public void MoveSelection(int delta)
    {
        var newIndex = _listView.SelectedItem + delta;
        newIndex = Math.Max(0, Math.Min(newIndex, _files.Count - 1));
        _listView.SelectedItem = newIndex;
    }
}

