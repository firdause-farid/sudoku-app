namespace Sudoku.Core.Domain.Model;

/// <summary>Applies row, column, and rectangular-subgrid Sudoku constraints.</summary>
internal static class SudokuConstraintChecker
{
    public static bool IsValidGrid(int?[,] values, SudokuDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(values);
        ValidateDimensions(values, definition);

        for (int row = 0; row < definition.GridSize; row++)
            for (int column = 0; column < definition.GridSize; column++)
                if (values[row, column] is int value && !definition.IsValueInRange(value))
                    return false;

        return !TryFindFirstViolation(values, definition, out _);
    }

    /// <summary>
    /// Looks for the first duplicate, checking every row, then every column, then every subgrid.
    /// This is the single place that decides which rules apply and in which order.
    /// </summary>
    public static bool TryFindFirstViolation(int?[,] values, SudokuDefinition definition, out ConstraintViolation violation)
    {
        for (int row = 0; row < definition.GridSize; row++)
            if (TryFindDuplicate(values, row, 0, 1, definition.GridSize, definition, out int duplicate))
            {
                violation = new ConstraintViolation(ConstraintUnit.Row, row, duplicate);
                return true;
            }

        for (int column = 0; column < definition.GridSize; column++)
            if (TryFindDuplicate(values, 0, column, definition.GridSize, 1, definition, out int duplicate))
            {
                violation = new ConstraintViolation(ConstraintUnit.Column, column, duplicate);
                return true;
            }

        int subgridsPerRow = definition.GridSize / definition.SubgridColumns;
        for (int startRow = 0; startRow < definition.GridSize; startRow += definition.SubgridRows)
            for (int startColumn = 0; startColumn < definition.GridSize; startColumn += definition.SubgridColumns)
                if (TryFindDuplicate(values, startRow, startColumn, definition.SubgridRows, definition.SubgridColumns, definition, out int duplicate))
                {
                    int subgridIndex = startRow / definition.SubgridRows * subgridsPerRow + startColumn / definition.SubgridColumns;
                    violation = new ConstraintViolation(ConstraintUnit.Subgrid, subgridIndex, duplicate);
                    return true;
                }

        violation = default;
        return false;
    }

    public static bool HasPlacementConflict(int?[,] values, int row, int column, int value, SudokuDefinition definition)
    {
        int subgridStartRow = row / definition.SubgridRows * definition.SubgridRows;
        int subgridStartColumn = column / definition.SubgridColumns * definition.SubgridColumns;

        return Contains(values, value, row, 0, 1, definition.GridSize, row, column)
            || Contains(values, value, 0, column, definition.GridSize, 1, row, column)
            || Contains(values, value, subgridStartRow, subgridStartColumn, definition.SubgridRows, definition.SubgridColumns, row, column);
    }

    private static void ValidateDimensions(int?[,] values, SudokuDefinition definition)
    {
        if (values.GetLength(0) != definition.GridSize || values.GetLength(1) != definition.GridSize)
            throw new ArgumentException($"Grid must be {definition.GridSize}x{definition.GridSize}.", nameof(values));
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
        SudokuDefinition definition,
        out int duplicate)
    {
        var seen = new bool[definition.GridSize + 1];
        for (int rowOffset = 0; rowOffset < rowCount; rowOffset++)
            for (int columnOffset = 0; columnOffset < columnCount; columnOffset++)
            {
                int? value = values[startRow + rowOffset, startColumn + columnOffset];
                if (value is not int number)
                    continue;
                if (number < 1 || number > definition.GridSize)
                    continue;
                if (seen[number])
                {
                    duplicate = number;
                    return true;
                }
                seen[number] = true;
            }

        duplicate = default;
        return false;
    }
}
