namespace File_Commander.Models;

/// <summary>
/// Represents a key binding for a command
/// </summary>
public class KeyBinding
{
    public string Command { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public List<string> Keys { get; set; } = new();

    public KeyBinding() { }

    public KeyBinding(string command, string displayName, params string[] keys)
    {
        Command = command;
        DisplayName = displayName;
        Keys = new List<string>(keys);
    }
}

