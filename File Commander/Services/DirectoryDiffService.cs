using File_Commander.Models;

namespace File_Commander.Services;

/// <summary>
/// Service for comparing directories and generating diff results
/// Phase 3: Diff/Sync mode implementation
/// </summary>
public class DirectoryDiffService
{
    /// <summary>
    /// Compares two directories and returns diff results
    /// </summary>
    /// <param name="leftPath">Left directory path</param>
    /// <param name="rightPath">Right directory path</param>
    /// <param name="isRecursive">Whether to recursively compare subdirectories</param>
    /// <returns>List of diff results</returns>
    public List<DiffResult> GetDirectoryDiff(string leftPath, string rightPath, bool isRecursive = false)
    {
        var results = new List<DiffResult>();

        try
        {
            var leftDir = new DirectoryInfo(leftPath);
            var rightDir = new DirectoryInfo(rightPath);

            if (!leftDir.Exists || !rightDir.Exists)
            {
                return results; // Empty result for invalid paths
            }

            // Build dictionaries for quick lookup
            var leftFiles = BuildFileMap(leftDir, isRecursive);
            var rightFiles = BuildFileMap(rightDir, isRecursive);

            // Find files in left, right, or both
            var allPaths = leftFiles.Keys.Union(rightFiles.Keys).ToHashSet();

            foreach (var relativePath in allPaths.OrderBy(p => p))
            {
                var leftExists = leftFiles.TryGetValue(relativePath, out var leftInfo);
                var rightExists = rightFiles.TryGetValue(relativePath, out var rightInfo);

                var diff = new DiffResult
                {
                    RelativePath = relativePath,
                    IsDirectory = leftInfo?.IsDirectory ?? rightInfo!.IsDirectory
                };

                if (leftExists && rightExists)
                {
                    // Both sides exist - compare
                    diff.LeftSize = leftInfo!.Size;
                    diff.LeftModified = leftInfo.Modified;
                    diff.LeftFullPath = leftInfo.FullPath;

                    diff.RightSize = rightInfo!.Size;
                    diff.RightModified = rightInfo.Modified;
                    diff.RightFullPath = rightInfo.FullPath;

                    // Determine diff type
                    if (leftInfo.Size == rightInfo.Size && leftInfo.Modified == rightInfo.Modified)
                    {
                        diff.DiffType = DiffType.Identical;
                        diff.RecommendedAction = SyncAction.None;
                    }
                    else if (leftInfo.Modified > rightInfo.Modified)
                    {
                        diff.DiffType = DiffType.LeftNewer;
                        diff.RecommendedAction = SyncAction.CopyLeftToRight;
                    }
                    else if (rightInfo.Modified > leftInfo.Modified)
                    {
                        diff.DiffType = DiffType.RightNewer;
                        diff.RecommendedAction = SyncAction.CopyRightToLeft;
                    }
                    else
                    {
                        diff.DiffType = DiffType.Conflict;
                        diff.RecommendedAction = SyncAction.Skip;
                    }
                }
                else if (leftExists)
                {
                    // Only on left
                    diff.DiffType = DiffType.LeftOnly;
                    diff.LeftSize = leftInfo!.Size;
                    diff.LeftModified = leftInfo.Modified;
                    diff.LeftFullPath = leftInfo.FullPath;
                    diff.RecommendedAction = SyncAction.CopyLeftToRight;
                }
                else
                {
                    // Only on right
                    diff.DiffType = DiffType.RightOnly;
                    diff.RightSize = rightInfo!.Size;
                    diff.RightModified = rightInfo.Modified;
                    diff.RightFullPath = rightInfo.FullPath;
                    diff.RecommendedAction = SyncAction.CopyRightToLeft;
                }

                results.Add(diff);
            }
        }
        catch (Exception ex)
        {
            // Add error result
            results.Add(new DiffResult
            {
                RelativePath = $"ERROR: {ex.Message}",
                DiffType = DiffType.Conflict,
                RecommendedAction = SyncAction.Skip
            });
        }

        return results;
    }

    /// <summary>
    /// Builds a map of relative paths to file info
    /// </summary>
    private Dictionary<string, FileInfoData> BuildFileMap(DirectoryInfo directory, bool recursive)
    {
        var map = new Dictionary<string, FileInfoData>();
        var basePath = directory.FullName;

        try
        {
            // Add files
            foreach (var file in directory.GetFiles())
            {
                try
                {
                    var relativePath = GetRelativePath(basePath, file.FullName);
                    map[relativePath] = new FileInfoData
                    {
                        FullPath = file.FullName,
                        Size = file.Length,
                        Modified = file.LastWriteTime,
                        IsDirectory = false
                    };
                }
                catch (UnauthorizedAccessException)
                {
                    // Skip inaccessible files
                }
            }

            // Add directories
            foreach (var dir in directory.GetDirectories())
            {
                try
                {
                    var relativePath = GetRelativePath(basePath, dir.FullName);
                    map[relativePath] = new FileInfoData
                    {
                        FullPath = dir.FullName,
                        Size = 0,
                        Modified = dir.LastWriteTime,
                        IsDirectory = true
                    };

                    // Recurse if needed
                    if (recursive)
                    {
                        var subMap = BuildFileMap(dir, recursive);
                        foreach (var kvp in subMap)
                        {
                            map[Path.Combine(relativePath, kvp.Key)] = kvp.Value;
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // Skip inaccessible directories
                }
            }
        }
        catch
        {
            // Return what we have
        }

        return map;
    }

    /// <summary>
    /// Gets relative path from base to target
    /// </summary>
    private string GetRelativePath(string basePath, string targetPath)
    {
        var baseUri = new Uri(basePath.EndsWith(Path.DirectorySeparatorChar.ToString())
            ? basePath
            : basePath + Path.DirectorySeparatorChar);
        var targetUri = new Uri(targetPath);
        var relativeUri = baseUri.MakeRelativeUri(targetUri);
        return Uri.UnescapeDataString(relativeUri.ToString()).Replace('/', Path.DirectorySeparatorChar);
    }

    /// <summary>
    /// Internal data structure for file info
    /// </summary>
    private class FileInfoData
    {
        public string FullPath { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime Modified { get; set; }
        public bool IsDirectory { get; set; }
    }
}

