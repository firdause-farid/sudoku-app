namespace Sudoku.Core.Domain.Model;

/// <summary>
/// Fixed dimensions and value range for a standard Sudoku board.
/// </summary>
public static class SudokuRules
{
    public const int GridSize = 9;
    public const int SubgridSize = 3;
    public const int MinimumValue = 1;
    public const int MaximumValue = GridSize;
    public const char FirstRowLabel = 'A';
    public const char FirstColumnLabel = '1';
    public const int FirstColumnNumber = 1;
    public const int ValueLookupLength = MaximumValue + 1;
}
