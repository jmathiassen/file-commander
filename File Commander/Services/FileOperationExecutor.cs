namespace File_Commander.Services;

/// <summary>
/// Executes individual file operations (refactored from FileOperationService)
/// Phase 2: Works with single jobs, called by IntelligentTaskQueueService
/// </summary>
public class FileOperationExecutor
{
    public event EventHandler<(Guid JobId, string Status)>? StatusChanged;
    public event EventHandler<(Guid JobId, int Progress)>? ProgressChanged;

    /// <summary>
    /// Copies a single file or directory
    /// </summary>
    public async Task<bool> CopySingleAsync(Guid jobId, string sourcePath, string destinationPath, CancellationToken cancellationToken = default)
    {
        try
        {
            StatusChanged?.Invoke(this, (jobId, $"Copying {Path.GetFileName(sourcePath)}"));
            ProgressChanged?.Invoke(this, (jobId, 0));

            if (Directory.Exists(sourcePath))
            {
                await CopyDirectoryAsync(jobId, sourcePath, destinationPath, cancellationToken);
            }
            else if (File.Exists(sourcePath))
            {
                await CopyFileAsync(jobId, sourcePath, destinationPath, cancellationToken);
            }
            else
            {
                StatusChanged?.Invoke(this, (jobId, $"Source not found: {sourcePath}"));
                return false;
            }

            ProgressChanged?.Invoke(this, (jobId, 100));
            StatusChanged?.Invoke(this, (jobId, $"Copied {Path.GetFileName(sourcePath)}"));
            return true;
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke(this, (jobId, $"Copy failed: {ex.Message}"));
            return false;
        }
    }

    /// <summary>
    /// Moves a single file or directory
    /// </summary>
    public async Task<bool> MoveSingleAsync(Guid jobId, string sourcePath, string destinationPath, CancellationToken cancellationToken = default)
    {
        try
        {
            StatusChanged?.Invoke(this, (jobId, $"Moving {Path.GetFileName(sourcePath)}"));
            ProgressChanged?.Invoke(this, (jobId, 0));

            await Task.Run(() =>
            {
                if (Directory.Exists(sourcePath))
                {
                    Directory.Move(sourcePath, destinationPath);
                }
                else if (File.Exists(sourcePath))
                {
                    File.Move(sourcePath, destinationPath, overwrite: false);
                }
                else
                {
                    throw new FileNotFoundException($"Source not found: {sourcePath}");
                }
            }, cancellationToken);

            ProgressChanged?.Invoke(this, (jobId, 100));
            StatusChanged?.Invoke(this, (jobId, $"Moved {Path.GetFileName(sourcePath)}"));
            return true;
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke(this, (jobId, $"Move failed: {ex.Message}"));
            return false;
        }
    }

    /// <summary>
    /// Deletes a single file or directory
    /// </summary>
    public async Task<bool> DeleteSingleAsync(Guid jobId, string path, CancellationToken cancellationToken = default)
    {
        try
        {
            StatusChanged?.Invoke(this, (jobId, $"Deleting {Path.GetFileName(path)}"));
            ProgressChanged?.Invoke(this, (jobId, 0));

            await Task.Run(() =>
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, recursive: true);
                }
                else if (File.Exists(path))
                {
                    File.Delete(path);
                }
                else
                {
                    throw new FileNotFoundException($"Path not found: {path}");
                }
            }, cancellationToken);

            ProgressChanged?.Invoke(this, (jobId, 100));
            StatusChanged?.Invoke(this, (jobId, $"Deleted {Path.GetFileName(path)}"));
            return true;
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke(this, (jobId, $"Delete failed: {ex.Message}"));
            return false;
        }
    }

    private async Task CopyFileAsync(Guid jobId, string sourcePath, string destPath, CancellationToken cancellationToken)
    {
        const int bufferSize = 81920; // 80KB buffer

        await using var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true);
        await using var destStream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true);

        await sourceStream.CopyToAsync(destStream, bufferSize, cancellationToken);
    }

    private async Task CopyDirectoryAsync(Guid jobId, string sourcePath, string destPath, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(destPath);

        var dirInfo = new DirectoryInfo(sourcePath);

        foreach (var file in dirInfo.GetFiles())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var destFile = Path.Combine(destPath, file.Name);
            await CopyFileAsync(jobId, file.FullName, destFile, cancellationToken);
        }

        foreach (var dir in dirInfo.GetDirectories())
        {
            cancellationToken.ThrowIfCancellationRequested();
            var destDir = Path.Combine(destPath, dir.Name);
            await CopyDirectoryAsync(jobId, dir.FullName, destDir, cancellationToken);
        }
    }
}

