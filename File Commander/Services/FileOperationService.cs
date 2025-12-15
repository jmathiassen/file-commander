using File_Commander.Models;

namespace File_Commander.Services;

/// <summary>
/// Service for performing file operations (Copy, Move, Delete)
/// Phase 1: Sequential operations without queue
/// </summary>
public class FileOperationService
{
    public event EventHandler<string>? StatusChanged;
    public event EventHandler<int>? ProgressChanged;

    /// <summary>
    /// Copies files/directories from source to destination
    /// </summary>
    public async Task<bool> CopyAsync(List<string> sourcePaths, string destinationPath, CancellationToken cancellationToken = default)
    {
        try
        {
            int total = sourcePaths.Count;
            int current = 0;

            foreach (var sourcePath in sourcePaths)
            {
                cancellationToken.ThrowIfCancellationRequested();

                current++;
                var fileName = Path.GetFileName(sourcePath);
                StatusChanged?.Invoke(this, $"Copying {fileName} ({current}/{total})");
                ProgressChanged?.Invoke(this, (current * 100) / total);

                if (Directory.Exists(sourcePath))
                {
                    await CopyDirectoryAsync(sourcePath, Path.Combine(destinationPath, fileName), cancellationToken);
                }
                else if (File.Exists(sourcePath))
                {
                    var destFile = Path.Combine(destinationPath, fileName);
                    await CopyFileAsync(sourcePath, destFile, cancellationToken);
                }
            }

            StatusChanged?.Invoke(this, "Copy completed");
            return true;
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke(this, $"Copy failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Moves files/directories from source to destination
    /// </summary>
    public Task<bool> MoveAsync(List<string> sourcePaths, string destinationPath, CancellationToken cancellationToken = default)
    {
        try
        {
            int total = sourcePaths.Count;
            int current = 0;

            foreach (var sourcePath in sourcePaths)
            {
                cancellationToken.ThrowIfCancellationRequested();

                current++;
                var fileName = Path.GetFileName(sourcePath);
                StatusChanged?.Invoke(this, $"Moving {fileName} ({current}/{total})");
                ProgressChanged?.Invoke(this, (current * 100) / total);

                var destPath = Path.Combine(destinationPath, fileName);

                if (Directory.Exists(sourcePath))
                {
                    Directory.Move(sourcePath, destPath);
                }
                else if (File.Exists(sourcePath))
                {
                    File.Move(sourcePath, destPath, overwrite: false);
                }
            }

            StatusChanged?.Invoke(this, "Move completed");
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke(this, $"Move failed: {ex.Message}");
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Deletes files/directories
    /// </summary>
    public async Task<bool> DeleteAsync(List<string> paths, CancellationToken cancellationToken = default)
    {
        try
        {
            int total = paths.Count;
            int current = 0;

            foreach (var path in paths)
            {
                cancellationToken.ThrowIfCancellationRequested();

                current++;
                var fileName = Path.GetFileName(path);
                StatusChanged?.Invoke(this, $"Deleting {fileName} ({current}/{total})");
                ProgressChanged?.Invoke(this, (current * 100) / total);

                if (Directory.Exists(path))
                {
                    await Task.Run(() => Directory.Delete(path, recursive: true), cancellationToken);
                }
                else if (File.Exists(path))
                {
                    await Task.Run(() => File.Delete(path), cancellationToken);
                }
            }

            StatusChanged?.Invoke(this, "Delete completed");
            return true;
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke(this, $"Delete failed: {ex.Message}");
            return false;
        }
    }

    private async Task CopyFileAsync(string sourcePath, string destPath, CancellationToken cancellationToken)
    {
        const int bufferSize = 81920; // 80KB buffer

        await using var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true);
        await using var destStream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true);

        await sourceStream.CopyToAsync(destStream, bufferSize, cancellationToken);
    }

    private async Task CopyDirectoryAsync(string sourcePath, string destPath, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(destPath);

        var dirInfo = new DirectoryInfo(sourcePath);

        foreach (var file in dirInfo.GetFiles())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var destFile = Path.Combine(destPath, file.Name);
            await CopyFileAsync(file.FullName, destFile, cancellationToken);
        }

        foreach (var dir in dirInfo.GetDirectories())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var destDir = Path.Combine(destPath, dir.Name);
            await CopyDirectoryAsync(dir.FullName, destDir, cancellationToken);
        }
    }
}

