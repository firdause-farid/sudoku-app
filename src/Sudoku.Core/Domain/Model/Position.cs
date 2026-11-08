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

    /// <summary>
    /// Parses text such as "B3" (row label followed by column number), ignoring case and surrounding whitespace.
    /// </summary>
    public static bool TryParse(string? text, SudokuDefinition definition, out Position position, out PositionParseError error)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return definition.TryParsePosition(text, out position, out error);
    }
}
