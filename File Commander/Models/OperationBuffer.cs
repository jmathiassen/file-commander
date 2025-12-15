namespace File_Commander.Models;

/// <summary>
/// Represents a file operation (Copy/Move/Delete) to be staged or executed
/// </summary>
public enum OperationType
{
    Copy,
    Move,
    Delete
}

/// <summary>
/// Staged operation buffer for Cut/Copy/Paste operations
/// </summary>
public class OperationBuffer
{
    public OperationType Operation { get; set; }
    public List<string> SourcePaths { get; set; } = new();
    public bool IsEmpty => SourcePaths.Count == 0;

    public void Clear()
    {
        SourcePaths.Clear();
    }
}

