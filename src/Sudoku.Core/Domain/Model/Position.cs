namespace Sudoku.Core.Domain.Model;

/// <summary>
/// A zero-based position on a Sudoku grid.
/// </summary>
public readonly record struct Position
{
    private const int TextLength = 2;

    public Position(int row, int column)
    {
        if (row is < 0 or >= SudokuRules.GridSize)
            throw new ArgumentOutOfRangeException(nameof(row), $"Row must be between 0 and {SudokuRules.GridSize - 1}.");
        if (column is < 0 or >= SudokuRules.GridSize)
            throw new ArgumentOutOfRangeException(nameof(column), $"Column must be between 0 and {SudokuRules.GridSize - 1}.");

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
    {
        position = default;

        if (string.IsNullOrWhiteSpace(text))
        {
            error = PositionParseError.Required;
            return false;
        }

        string normalized = text.Trim().ToUpperInvariant();
        if (normalized.Length < TextLength)
        {
            error = PositionParseError.InvalidFormat;
            return false;
        }

        char rowChar = normalized[0];
        if (rowChar < SudokuRules.FirstRowLabel || rowChar >= SudokuRules.FirstRowLabel + SudokuRules.GridSize)
        {
            error = PositionParseError.RowOutOfRange;
            return false;
        }

        string columnText = normalized.Substring(1);
        if (!int.TryParse(columnText, out int columnNumber))
        {
            error = PositionParseError.InvalidFormat;
            return false;
        }

        if (columnNumber < SudokuRules.FirstColumnNumber || columnNumber > SudokuRules.GridSize)
        {
            error = PositionParseError.ColumnOutOfRange;
            return false;
        }

        position = new Position(rowChar - SudokuRules.FirstRowLabel, columnNumber - SudokuRules.FirstColumnNumber);
        error = PositionParseError.None;
        return true;
    }

    public override string ToString() => $"{RowLetter}{ColumnNumber}";
}
