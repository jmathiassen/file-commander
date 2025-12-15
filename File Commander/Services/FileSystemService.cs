using File_Commander.Models;

namespace File_Commander.Services;

/// <summary>
/// Service for reading and navigating the file system
/// </summary>
public class FileSystemService
{
    /// <summary>
    /// Lists all files and directories in the specified path
    /// </summary>
    public List<FileItem> ListDirectory(string path, bool showHidden = false)
    {
        var items = new List<FileItem>();

        try
        {
            var dirInfo = new DirectoryInfo(path);

            // Add parent directory entry (..)
            if (dirInfo.Parent != null)
            {
                items.Add(new FileItem
                {
                    Name = "..",
                    FullPath = dirInfo.Parent.FullName,
                    IsDirectory = true,
                    Size = 0,
                    LastModified = DateTime.MinValue,
                    Attributes = FileAttributes.Directory
                });
            }

            // Add directories first
            foreach (var dir in dirInfo.GetDirectories())
            {
                try
                {
                    // Filter hidden directories unless showHidden is true
                    if (!showHidden && (dir.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                    {
                        continue;
                    }

                    items.Add(new FileItem
                    {
                        Name = dir.Name,
                        FullPath = dir.FullName,
                        IsDirectory = true,
                        Size = 0,
                        LastModified = dir.LastWriteTime,
                        Attributes = dir.Attributes
                    });
                }
                catch (UnauthorizedAccessException)
                {
                    // Skip directories we can't access
                }
            }

            // Add files
            foreach (var file in dirInfo.GetFiles())
            {
                try
                {
                    // Filter hidden files unless showHidden is true
                    if (!showHidden && (file.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                    {
                        continue;
                    }

                    items.Add(new FileItem
                    {
                        Name = file.Name,
                        FullPath = file.FullName,
                        IsDirectory = false,
                        Size = file.Length,
                        LastModified = file.LastWriteTime,
                        Attributes = file.Attributes
                    });
                }
                catch (UnauthorizedAccessException)
                {
                    // Skip files we can't access
                }
            }
        }
        catch (Exception ex)
        {
            // Return error item
            items.Add(new FileItem
            {
                Name = $"ERROR: {ex.Message}",
                FullPath = path,
                IsDirectory = false,
                Size = 0,
                LastModified = DateTime.MinValue
            });
        }

        return items;
    }

    /// <summary>
    /// Gets available drives/mount points
    /// </summary>
    public List<FileItem> GetDrives()
    {
        var drives = new List<FileItem>();

        try
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    drives.Add(new FileItem
                    {
                        Name = $"{drive.Name} [{drive.VolumeLabel}] ({drive.DriveType})",
                        FullPath = drive.RootDirectory.FullName,
                        IsDirectory = true,
                        Size = drive.TotalSize,
                        LastModified = DateTime.MinValue,
                        Attributes = FileAttributes.Directory
                    });
                }
            }
        }
        catch (Exception)
        {
            // Fallback to root
            drives.Add(new FileItem
            {
                Name = "/",
                FullPath = "/",
                IsDirectory = true,
                Size = 0,
                LastModified = DateTime.MinValue,
                Attributes = FileAttributes.Directory
            });
        }

        return drives;
    }

    /// <summary>
    /// Normalizes a path and ensures it exists
    /// </summary>
    public string NormalizePath(string path)
    {
        try
        {
            var normalized = Path.GetFullPath(path);
            return Directory.Exists(normalized) ? normalized : Environment.CurrentDirectory;
        }
        catch
        {
            return Environment.CurrentDirectory;
        }
    }

    /// <summary>
    /// Calculates the total size of a directory recursively
    /// </summary>
    public long CalculateDirectorySize(string path)
    {
        try
        {
            var dirInfo = new DirectoryInfo(path);
            long size = 0;

            // Sum all files in this directory
            foreach (var file in dirInfo.GetFiles())
            {
                try
                {
                    size += file.Length;
                }
                catch (UnauthorizedAccessException)
                {
                    // Skip files we can't access
                }
            }

            // Recursively sum subdirectories
            foreach (var dir in dirInfo.GetDirectories())
            {
                try
                {
                    size += CalculateDirectorySize(dir.FullName);
                }
                catch (UnauthorizedAccessException)
                {
                    // Skip directories we can't access
                }
            }

            return size;
        }
        catch
        {
            return 0;
        }
    }
}

