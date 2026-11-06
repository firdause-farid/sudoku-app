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
        sb.Append(_style.ColumnHeaderPrefix)
            .Append(_style.FormatColumnHeader(UserMessages.GridHeader));

        if (_style.HeaderSeparator is { } headerSeparator)
            sb.AppendLine().Append(headerSeparator);

        for (int r = 0; r < SudokuRules.GridSize; r++)
        {
            var rowPosition = new Position(r, 0);
            sb.AppendLine()
                .Append("  ");
            sb.Append(_style.FormatRowHeader(rowPosition.RowLetter.ToString()))
                .Append(_style.RowHeaderSeparator);

            for (int c = 0; c < SudokuRules.GridSize; c++)
            {
                var position = new Position(r, c);
                int? value = grid.GetValue(position);
                bool isPrefilled = grid.IsPrefilled(position);

                string text = value.HasValue ? value.Value.ToString() : UserMessages.EmptyCell;
                sb.Append(isPrefilled ? _style.FormatPrefilledCell(text) : text);
                if (c < SudokuRules.GridSize - 1)
                    sb.Append(' ');
            }
        }

        return sb.ToString();
    }
}

