namespace File_Commander.Models;

/// <summary>
/// Represents the type of difference between two files/directories
/// Phase 3: Diff/Sync mode foundation
/// </summary>
public enum DiffType
{
    /// <summary>
    /// Files are identical (same size, same timestamp)
    /// </summary>
    Identical,

    /// <summary>
    /// File exists only on the left side
    /// </summary>
    LeftOnly,

    /// <summary>
    /// File exists only on the right side
    /// </summary>
    RightOnly,

    /// <summary>
    /// File exists on both sides but differs (size or timestamp)
    /// </summary>
    Conflict,

    /// <summary>
    /// Left file is newer
    /// </summary>
    LeftNewer,

    /// <summary>
    /// Right file is newer
    /// </summary>
    RightNewer
}

/// <summary>
/// Represents a difference between two files or directories
/// Phase 3: Diff/Sync mode foundation
/// </summary>
public class DiffResult
{
    public string RelativePath { get; set; } = string.Empty;
    public DiffType DiffType { get; set; }
    public bool IsDirectory { get; set; }

    // Left side info
    public long? LeftSize { get; set; }
    public DateTime? LeftModified { get; set; }
    public string? LeftFullPath { get; set; }

    // Right side info
    public long? RightSize { get; set; }
    public DateTime? RightModified { get; set; }
    public string? RightFullPath { get; set; }

    /// <summary>
    /// Recommended sync action
    /// </summary>
    public SyncAction RecommendedAction { get; set; }
}

/// <summary>
/// Actions that can be taken to synchronize files
/// Phase 3: Diff/Sync mode foundation
/// </summary>
public enum SyncAction
{
    None,
    CopyLeftToRight,
    CopyRightToLeft,
    DeleteLeft,
    DeleteRight,
    Skip  // User chooses to skip this conflict
}

