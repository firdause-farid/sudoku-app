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
        var sb = new System.Text.StringBuilder();
        string columnHeader = string.Join(" ", Enumerable.Range(1, grid.Definition.GridSize));
        sb.Append(_style.ColumnHeaderPrefix)
            .Append(_style.FormatColumnHeader(columnHeader));

        if (_style.GetHeaderSeparator(grid.Definition.GridSize) is { } headerSeparator)
            sb.AppendLine().Append(headerSeparator);

        for (int r = 0; r < grid.Definition.GridSize; r++)
        {
            sb.AppendLine()
                .Append("  ");
            sb.Append(_style.FormatRowHeader(grid.Definition.RowLabels[r]))
                .Append(_style.RowHeaderSeparator);

            for (int c = 0; c < grid.Definition.GridSize; c++)
            {
                var position = new Position(r, c);
                int? value = grid.GetValue(position);
                bool isPrefilled = grid.IsPrefilled(position);

                string text = value.HasValue ? grid.Definition.FormatValue(value.Value) : UserMessages.EmptyCell;
                sb.Append(isPrefilled ? _style.FormatPrefilledCell(text) : text);
                if (c < grid.Definition.GridSize - 1)
                    sb.Append(' ');
            }
        }

        return sb.ToString();
    }
}

