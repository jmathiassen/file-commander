namespace File_Commander.Models;

/// <summary>
/// Represents a file or directory entry in the file system
/// </summary>
public class FileItem
{
    public string Name { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public long Size { get; set; }
    public bool IsDirectory { get; set; }
    public DateTime LastModified { get; set; }
    public FileAttributes Attributes { get; set; }

    /// <summary>
    /// For directories, stores the calculated size if available
    /// </summary>
    public long? CalculatedSize { get; set; }

    /// <summary>
    /// Formatted size string (e.g., "1.5 MB", "DIR")
    /// </summary>
    public string FormattedSize
    {
        get
        {
            if (IsDirectory)
            {
                if (CalculatedSize.HasValue)
                {
                    return FormatBytes(CalculatedSize.Value, FileSizeFormat.KiB);
                }
                return "<DIR>";
            }
            return FormatBytes(Size, FileSizeFormat.KiB);
        }
    }

    /// <summary>
    /// Gets formatted size with specific format
    /// </summary>
    public string GetFormattedSize(FileSizeFormat format)
    {
        if (IsDirectory)
        {
            if (CalculatedSize.HasValue)
            {
                return FormatBytes(CalculatedSize.Value, format);
            }
            return "<DIR>";
        }
        return FormatBytes(Size, format);
    }

    /// <summary>
    /// Formatted date string
    /// </summary>
    public string FormattedDate => LastModified.ToString("yyyy-MM-dd HH:mm");

    /// <summary>
    /// Gets formatted date with optional seconds
    /// </summary>
    public string GetFormattedDate(bool showSeconds)
    {
        return showSeconds
            ? LastModified.ToString("yyyy-MM-dd HH:mm:ss")
            : LastModified.ToString("yyyy-MM-dd HH:mm");
    }

    private static string FormatBytes(long bytes, FileSizeFormat format)
    {
        if (format == FileSizeFormat.Bytes)
        {
            // Format with thousands separator
            return bytes.ToString("N0").Replace(",", "'");
        }

        string[] sizes;
        int divisor;

        if (format == FileSizeFormat.KB)
        {
            sizes = new[] { "B", "KB", "MB", "GB", "TB" };
            divisor = 1000;
        }
        else // FileSizeFormat.KiB
        {
            sizes = new[] { "B", "KiB", "MiB", "GiB", "TiB" };
            divisor = 1024;
        }

        int order = 0;
        double size = bytes;

        while (size >= divisor && order < sizes.Length - 1)
        {
            order++;
            size /= divisor;
        }

        return $"{size:0.##} {sizes[order]}";
    }
}

