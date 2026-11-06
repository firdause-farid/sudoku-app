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
    public char RowLetter => (char)(SudokuRules.FirstRowLabel + Row);
    public int ColumnNumber => Column + SudokuRules.FirstColumnNumber;

    /// <summary>
    /// Parses text such as "B3" (row letter followed by column number), ignoring case and surrounding whitespace.
    /// </summary>
    public static bool TryParse(string? text, out Position position, out PositionParseError error)
        => TryParse(text, SudokuDefinition.Classic, out position, out error);

    public static bool TryParse(string? text, SudokuDefinition definition, out Position position, out PositionParseError error)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return definition.TryParsePosition(text, out position, out error);
    }

    public override string ToString() => $"{RowLetter}{ColumnNumber}";
}
