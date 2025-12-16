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
    public DirectoryUpdateMode DirectoryUpdateMode { get; set; } = DirectoryUpdateMode.Manual;

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
}

