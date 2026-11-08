namespace Sudoku.Core.Domain.Model;

/// <summary>
/// A zero-based position on a Sudoku grid.
/// </summary>
public readonly record struct Position
{
    public Position(int row, int column)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(row);
        ArgumentOutOfRangeException.ThrowIfNegative(column);

        Row = row;
        Column = column;
    }

    public int Row { get; }
    public int Column { get; }
}
