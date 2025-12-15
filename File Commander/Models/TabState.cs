namespace File_Commander.Models;

/// <summary>
/// Represents the state of a single tab in the file manager
/// </summary>
public class TabState
{
    public Guid TabId { get; set; } = Guid.NewGuid();
    public string CurrentPath { get; set; } = string.Empty;
    public string PathPassive { get; set; } = string.Empty; // Used in DualPane mode
    public DisplayMode DisplayMode { get; set; } = DisplayMode.DualPane;
    public bool IsDirty { get; set; } = false;

    // Active pane selection state
    public int SelectedIndexActive { get; set; } = 0;
    public int ScrollOffsetActive { get; set; } = 0;
    public List<FileItem> FilesActive { get; set; } = new();

    // Passive pane selection state (DualPane mode only)
    public int SelectedIndexPassive { get; set; } = 0;
    public int ScrollOffsetPassive { get; set; } = 0;
    public List<FileItem> FilesPassive { get; set; } = new();

    // Track which pane is active (true = left/active, false = right/passive)
    public bool IsLeftPaneActive { get; set; } = true;

    // Selected files for operations (Ctrl+Space, Insert, etc.)
    public HashSet<string> MarkedFiles { get; set; } = new();

    public TabState(string initialPath)
    {
        CurrentPath = initialPath;
        PathPassive = initialPath;
    }
}

