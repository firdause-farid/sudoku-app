namespace Sudoku.Core.Presentation.Rendering;

/// <summary>
/// Formats a grid with ANSI emphasis and coordinate separators.
/// </summary>
public sealed class EnhancedGridStyle : IGridStyle
{
    private const string Dim = "\u001b[2m";
    private const string Cyan = "\u001b[36m";
    private const string Bold = "\u001b[1m";
    private const string BrightYellow = "\u001b[93m";
    private const string Reset = "\u001b[0m";

    public string ColumnHeaderPrefix => "    | ";
    public string RowHeaderSeparator => " | ";
    public string? GetHeaderSeparator(int gridSize) => $"----+{new string('-', gridSize * 2)}";

    public string FormatColumnHeader(string text) => Style(text, Dim);
    public string FormatRowHeader(string text) => Style(text, Cyan);
    public string FormatPrefilledCell(string text) => Style(text, $"{Bold}{BrightYellow}");

    private static string Style(string text, string style) => $"{style}{text}{Reset}";
}
