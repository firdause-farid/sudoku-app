using System.Text;
using Sudoku.Core.Application.Ports;
using Sudoku.Core.Domain.Model;
using Sudoku.Core.Presentation.Messages;

namespace Sudoku.Core.Presentation.Rendering;

/// <summary>
/// Renders a Sudoku grid using an injected presentation style.
/// </summary>
public sealed class GridRenderer : IRenderer
{
    private readonly IGridStyle _style;

    public GridRenderer(IGridStyle style)
    {
        _style = style ?? throw new ArgumentNullException(nameof(style));
    }

    public string Render(IGridView grid)
    {
        var builder = new StringBuilder();
        AppendHeader(builder, grid.Definition);

        for (int row = 0; row < grid.Definition.GridSize; row++)
            AppendRow(builder, grid, row);

        return builder.ToString();
    }

    private void AppendHeader(StringBuilder builder, SudokuDefinition definition)
    {
        string columnNumbers = string.Join(" ", Enumerable.Range(1, definition.GridSize));
        builder.Append(_style.ColumnHeaderPrefix)
            .Append(_style.FormatColumnHeader(columnNumbers));

        if (_style.GetHeaderSeparator(definition.GridSize) is { } headerSeparator)
            builder.AppendLine().Append(headerSeparator);
    }

    private void AppendRow(StringBuilder builder, IGridView grid, int row)
    {
        SudokuDefinition definition = grid.Definition;
        builder.AppendLine()
            .Append("  ")
            .Append(_style.FormatRowHeader(definition.RowLabels[row]))
            .Append(_style.RowHeaderSeparator);

        for (int column = 0; column < definition.GridSize; column++)
        {
            AppendCell(builder, grid, new Position(row, column));
            if (column < definition.GridSize - 1)
                builder.Append(' ');
        }
    }

    private void AppendCell(StringBuilder builder, IGridView grid, Position position)
    {
        int? value = grid.GetValue(position);
        string text = value.HasValue ? grid.Definition.FormatValue(value.Value) : UserMessages.EmptyCell;
        builder.Append(grid.IsPrefilled(position) ? _style.FormatPrefilledCell(text) : text);
    }
}
