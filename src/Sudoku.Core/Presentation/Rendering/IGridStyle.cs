namespace Sudoku.Core.Presentation.Rendering;

/// <summary>
/// Defines how grid components are formatted for a particular presentation style.
/// </summary>
public interface IGridStyle
{
    string ColumnHeaderPrefix { get; }
    string RowHeaderSeparator { get; }
    string? GetHeaderSeparator(int gridSize);

    string FormatColumnHeader(string text);
    string FormatRowHeader(string text);
    string FormatPrefilledCell(string text);
}
