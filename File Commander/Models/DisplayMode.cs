namespace File_Commander.Models;

/// <summary>
/// Defines the display mode for the file manager interface
/// </summary>
public enum DisplayMode
{
    /// <summary>
    /// Single pane mode showing one file list with optional tree/preview
    /// </summary>
    SinglePane,

    /// <summary>
    /// Dual pane mode for OFM-style operations (copy/move between panes)
    /// </summary>
    DualPane,

    /// <summary>
    /// Dual pane mode for directory comparison and synchronization (Phase 3)
    /// </summary>
    DualPane_DiffSync
}

