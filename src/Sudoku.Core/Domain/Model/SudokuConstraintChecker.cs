namespace Sudoku.Core.Domain.Model;

/// <summary>
/// Applies the shared row, column, and subgrid constraints used by validation and solving.
/// </summary>
internal static class SudokuConstraintChecker
{
    public static bool IsValidGrid(int?[,] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        ValidateDimensions(values);

        for (int row = 0; row < SudokuRules.GridSize; row++)
        {
            for (int column = 0; column < SudokuRules.GridSize; column++)
            {
                if (values[row, column] is int value &&
                    (value < SudokuRules.MinimumValue || value > SudokuRules.MaximumValue))
                    return false;
            }
        }

        for (int row = 0; row < SudokuRules.GridSize; row++)
            if (TryFindDuplicateInRow(values, row, out _))
                return false;

        for (int column = 0; column < SudokuRules.GridSize; column++)
            if (TryFindDuplicateInColumn(values, column, out _))
                return false;

        for (int startRow = 0; startRow < SudokuRules.GridSize; startRow += SudokuRules.SubgridSize)
            for (int startColumn = 0; startColumn < SudokuRules.GridSize; startColumn += SudokuRules.SubgridSize)
                if (TryFindDuplicateInSubgrid(values, startRow, startColumn, out _))
                    return false;

        return true;
    }

    public static bool HasPlacementConflict(int?[,] values, int row, int column, int value)
    {
        int boxRow = row / SudokuRules.SubgridSize * SudokuRules.SubgridSize;
        int boxColumn = column / SudokuRules.SubgridSize * SudokuRules.SubgridSize;

        return Contains(values, value, row, 0, 1, SudokuRules.GridSize, row, column)
            || Contains(values, value, 0, column, SudokuRules.GridSize, 1, row, column)
            || Contains(values, value, boxRow, boxColumn, SudokuRules.SubgridSize, SudokuRules.SubgridSize, row, column);
    }

    public static bool TryFindDuplicateInRow(int?[,] values, int row, out int duplicate)
    {
        return TryFindDuplicate(values, row, 0, 1, SudokuRules.GridSize, out duplicate);
    }

    public static bool TryFindDuplicateInColumn(int?[,] values, int column, out int duplicate)
    {
        return TryFindDuplicate(values, 0, column, SudokuRules.GridSize, 1, out duplicate);
    }

    public static bool TryFindDuplicateInSubgrid(int?[,] values, int startRow, int startColumn, out int duplicate)
    {
        return TryFindDuplicate(values, startRow, startColumn, SudokuRules.SubgridSize, SudokuRules.SubgridSize, out duplicate);
    }

    private static void ValidateDimensions(int?[,] values)
    {
        if (values.GetLength(0) != SudokuRules.GridSize || values.GetLength(1) != SudokuRules.GridSize)
            throw new ArgumentException($"Grid must be {SudokuRules.GridSize}x{SudokuRules.GridSize}.", nameof(values));
    }

    private static bool Contains(
        int?[,] values,
        int value,
        int startRow,
        int startColumn,
        int rowCount,
        int columnCount,
        int excludedRow,
        int excludedColumn)
    {
        for (int rowOffset = 0; rowOffset < rowCount; rowOffset++)
            for (int columnOffset = 0; columnOffset < columnCount; columnOffset++)
            {
                int row = startRow + rowOffset;
                int column = startColumn + columnOffset;
                if ((row != excludedRow || column != excludedColumn) && values[row, column] == value)
                    return true;
            }

        return false;
    }

    private static bool TryFindDuplicate(
        int?[,] values,
        int startRow,
        int startColumn,
        int rowCount,
        int columnCount,
        out int duplicate)
    {
        Span<bool> seen = stackalloc bool[SudokuRules.ValueLookupLength];
        for (int rowOffset = 0; rowOffset < rowCount; rowOffset++)
        {
            for (int columnOffset = 0; columnOffset < columnCount; columnOffset++)
            {
                int? value = values[startRow + rowOffset, startColumn + columnOffset];
                if (value is not int number)
                    continue;

                if (seen[number])
                {
                    duplicate = number;
                    return true;
                }

                seen[number] = true;
            }
        }

        duplicate = default;
        return false;
    }
}
