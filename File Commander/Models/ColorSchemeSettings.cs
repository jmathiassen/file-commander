namespace File_Commander.Models;

/// <summary>
/// Custom color scheme for different UI elements
/// </summary>
public class ColorSchemeSettings
{
    public ColorName NormalBackground { get; set; } = ColorName.Blue;
    public ColorName NormalForeground { get; set; } = ColorName.White;
    public ColorName SelectedBackground { get; set; } = ColorName.Cyan;
    public ColorName SelectedForeground { get; set; } = ColorName.Black;
    public ColorName DirectoryForeground { get; set; } = ColorName.BrightCyan;
    public ColorName ExecutableForeground { get; set; } = ColorName.BrightGreen;
    public ColorName MarkedForeground { get; set; } = ColorName.Yellow;
    public ColorName InactivePaneForeground { get; set; } = ColorName.Gray;
}

/// <summary>
/// Color names for Terminal.Gui
/// </summary>
public enum ColorName
{
    Black = 0,
    Blue = 1,
    Green = 2,
    Cyan = 3,
    Red = 4,
    Magenta = 5,
    Brown = 6,
    Gray = 7,
    DarkGray = 8,
    BrightBlue = 9,
    BrightGreen = 10,
    BrightCyan = 11,
    BrightRed = 12,
    BrightMagenta = 13,
    Yellow = 14,
    White = 15
}

