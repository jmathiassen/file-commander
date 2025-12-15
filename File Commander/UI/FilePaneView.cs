using Terminal.Gui;
using File_Commander.Models;

namespace File_Commander.UI;

/// <summary>
/// View for displaying a file list pane
/// </summary>
public class FilePaneView : FrameView
{
    private readonly ListView _listView;
    private List<FileItem> _files = new();
    private HashSet<string> _markedFiles = new();
    private bool _isActive = true;

    public event EventHandler<FileItem>? FileSelected;
    public event EventHandler<FileItem>? FileActivated;

    public FilePaneView(string title) : base(title)
    {
        _listView = new ListView
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            AllowsMarking = false,
            AllowsMultipleSelection = false
        };

        _listView.SelectedItemChanged += OnSelectedItemChanged;
        _listView.OpenSelectedItem += OnOpenSelectedItem;

        Add(_listView);
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

    public void SetFiles(List<FileItem> files, HashSet<string> markedFiles, int selectedIndex = 0, bool useNarrowIcons = true, bool showSeconds = true)
    {
        _files = files;
        _markedFiles = markedFiles;

        // Calculate available width for filename
        var dateWidth = showSeconds ? 19 : 16; // "yyyy-MM-dd HH:mm:ss" or "yyyy-MM-dd HH:mm"
        var sizeWidth = 12; // Right-aligned size column
        var markWidth = 2; // "* " or "  "
        var iconWidth = 2; // "D " or "F " (narrow) or wider for emoji
        var separators = 4; // Spaces between columns

        var availableWidth = Bounds.Width > 0 ? Bounds.Width : 80;
        var maxNameWidth = availableWidth - dateWidth - sizeWidth - markWidth - iconWidth - separators;
        if (maxNameWidth < 10) maxNameWidth = 10; // Minimum filename width

        var displayItems = files.Select(f =>
        {
            var mark = markedFiles.Contains(f.FullPath) ? "*" : " ";
            var icon = useNarrowIcons
                ? (f.IsDirectory ? "D" : "F")
                : (f.IsDirectory ? "📁" : "📄");

            // Truncate filename if needed
            var name = f.Name.Length > maxNameWidth
                ? f.Name.Substring(0, maxNameWidth - 3) + "..."
                : f.Name;

            // Left-align name, right-align size and date
            var namePadded = name.PadRight(maxNameWidth);
            var sizePadded = f.FormattedSize.PadLeft(sizeWidth);
            var datePadded = f.GetFormattedDate(showSeconds);

            return $"{mark} {icon} {namePadded} {sizePadded} {datePadded}";
        }).ToList();

        _listView.SetSource(displayItems);

        // CRITICAL FIX: Always synchronize selection index with TabState
        _listView.SelectedItem = Math.Min(selectedIndex, Math.Max(0, displayItems.Count - 1));
    }

    public void SetActive(bool isActive)
    {
        _isActive = isActive;
        ColorScheme = isActive ? Colors.Base : Colors.TopLevel;
    }

    public int SelectedIndex => _listView.SelectedItem;

    public void MoveSelection(int delta)
    {
        var newIndex = _listView.SelectedItem + delta;
        newIndex = Math.Max(0, Math.Min(newIndex, _files.Count - 1));
        _listView.SelectedItem = newIndex;
    }
}

