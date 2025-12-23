namespace File_Commander.Models;

/// <summary>
/// Directory update mode configuration
/// </summary>
public enum DirectoryUpdateMode
{
    /// <summary>
    /// Manual refresh only (F5)
    /// </summary>
    Manual = 0,

    /// <summary>
    /// Automatic refresh for active tab only
    /// </summary>
    ActiveTabOnly = 1,

    /// <summary>
    /// Automatic refresh for all tabs
    /// </summary>
    AllTabs = 2
}

/// <summary>
/// Application settings/configuration
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Directory update mode
    /// </summary>
    public DirectoryUpdateMode DirectoryUpdateMode { get; set; } = DirectoryUpdateMode.ActiveTabOnly;

    /// <summary>
    /// Show seconds in file date display
    /// </summary>
    public bool ShowSecondsInDate { get; set; } = true;

    /// <summary>
    /// Show hidden files and directories
    /// </summary>
    public bool ShowHiddenFiles { get; set; } = false;

    /// <summary>
    /// Follow symbolic links
    /// </summary>
    public bool FollowSymlinks { get; set; } = false;

    /// <summary>
    /// Show file/directory icons (D/F prefix)
    /// </summary>
    public bool ShowFileIcons { get; set; } = false;

    /// <summary>
    /// Use narrow icons (single-width characters) instead of emoji
    /// </summary>
    public bool UseNarrowIcons { get; set; } = true;

    /// <summary>
    /// Show file extensions in a separate column (Total Commander style)
    /// </summary>
    public bool ShowExtensionsInColumn { get; set; } = false;

    /// <summary>
    /// Auto-calculate directory sizes on marking
    /// </summary>
    public bool AutoCalculateDirectorySize { get; set; } = true;

    /// <summary>
    /// Automatically start queue when jobs are added (false = manual start required)
    /// </summary>
    public bool AutoStartQueue { get; set; } = true;

    /// <summary>
    /// File size format: 0=Bytes (with ' separator), 1=KB (1000-based), 2=KiB (1024-based)
    /// </summary>
    public FileSizeFormat FileSizeFormat { get; set; } = FileSizeFormat.KiB;

    /// <summary>
    /// Key bindings for commands
    /// </summary>
    public Dictionary<string, KeyBinding> KeyBindings { get; set; } = GetDefaultKeyBindings();

    /// <summary>
    /// Color scheme settings
    /// </summary>
    public ColorSchemeSettings ColorScheme { get; set; } = new();

    private static Dictionary<string, KeyBinding> GetDefaultKeyBindings()
    {
        return new Dictionary<string, KeyBinding>
        {
            ["Refresh"] = new("Refresh", "Refresh", "F5"),
            ["Copy"] = new("Copy", "Copy", "F5"),
            ["Move"] = new("Move", "Move/Rename", "F6"),
            ["CreateDirectory"] = new("CreateDirectory", "Create Directory", "F7"),
            ["Delete"] = new("Delete", "Delete", "F8"),
            ["Quit"] = new("Quit", "Quit", "F10", "Ctrl+Q"),
            ["Options"] = new("Options", "Options", "Ctrl+O"),
            ["Mark"] = new("Mark", "Mark/Unmark", "Insert", "Space"),
            ["MarkAll"] = new("MarkAll", "Mark All", "Ctrl+A"),
            ["UnmarkAll"] = new("UnmarkAll", "Unmark All", "Ctrl+U"),
            ["InvertSelection"] = new("InvertSelection", "Invert Selection", "Ctrl+I"),
            ["CalculateSize"] = new("CalculateSize", "Calculate Directory Size", "Ctrl+L"),
            ["ViewFile"] = new("ViewFile", "View File", "F3"),
            ["EditFile"] = new("EditFile", "Edit File", "F4"),
            ["Search"] = new("Search", "Search", "Ctrl+F"),
            ["GoToParent"] = new("GoToParent", "Go to Parent", "Backspace"),
            ["SwitchPane"] = new("SwitchPane", "Switch Pane", "Tab"),
            ["ShowHidden"] = new("ShowHidden", "Toggle Hidden Files", "Ctrl+H")
        };
    }
}

/// <summary>
/// File size display format
/// </summary>
public enum FileSizeFormat
{
    /// <summary>
    /// Show raw bytes with thousands separator (')
    /// </summary>
    Bytes = 0,

    /// <summary>
    /// Show KB, MB, GB (1000-based)
    /// </summary>
    KB = 1,

    /// <summary>
    /// Show KiB, MiB, GiB (1024-based)
    /// </summary>
    KiB = 2
}

