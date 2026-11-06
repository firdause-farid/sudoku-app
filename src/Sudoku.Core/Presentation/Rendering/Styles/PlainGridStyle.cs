namespace Sudoku.Core.Presentation.Rendering;

/// <summary>
/// Formats a grid without terminal styling or structural separators.
/// </summary>
public sealed class PlainGridStyle : IGridStyle
{
    public string ColumnHeaderPrefix => "    ";
    public string RowHeaderSeparator => " ";
    public string? GetHeaderSeparator(int gridSize) => null;

    public string FormatColumnHeader(string text) => text;
    public string FormatRowHeader(string text) => text;
    public string FormatPrefilledCell(string text) => text;
}
