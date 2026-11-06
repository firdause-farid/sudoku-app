using Sudoku.Core.Domain.Model;

namespace Sudoku.Core.Domain.Validation;

/// <summary>
/// Validates Sudoku grids for rule violations.
/// </summary>
public sealed class SudokuValidator : IValidator
{
    public ValidationResult Validate(IGridView grid)
    {
        ArgumentNullException.ThrowIfNull(grid);
        int?[,] values = ToValueArray(grid);

        // Rows
        for (int r = 0; r < SudokuRules.GridSize; r++)
        {
            if (SudokuConstraintChecker.TryFindDuplicateInRow(values, r, out int duplicate))
                return ValidationResult.Invalid(
                    new ValidationIssue(ValidationIssueType.DuplicateRow, duplicate, r));
        }

        // Columns
        for (int c = 0; c < SudokuRules.GridSize; c++)
        {
            if (SudokuConstraintChecker.TryFindDuplicateInColumn(values, c, out int duplicate))
                return ValidationResult.Invalid(
                    new ValidationIssue(ValidationIssueType.DuplicateColumn, duplicate, c));
        }

        // 3x3 subgrids
        for (int boxRow = 0; boxRow < SudokuRules.GridSize / SudokuRules.SubgridSize; boxRow++)
        {
            for (int boxCol = 0; boxCol < SudokuRules.GridSize / SudokuRules.SubgridSize; boxCol++)
            {
                int startRow = boxRow * SudokuRules.SubgridSize;
                int startColumn = boxCol * SudokuRules.SubgridSize;
                if (SudokuConstraintChecker.TryFindDuplicateInSubgrid(values, startRow, startColumn, out int duplicate))
                    return ValidationResult.Invalid(
                        new ValidationIssue(ValidationIssueType.DuplicateSubgrid, duplicate));
            }
        }

        return ValidationResult.Valid();
    }

    private static int?[,] ToValueArray(IGridView grid)
    {
        var values = new int?[SudokuRules.GridSize, SudokuRules.GridSize];
        for (int row = 0; row < SudokuRules.GridSize; row++)
            for (int column = 0; column < SudokuRules.GridSize; column++)
                values[row, column] = grid.GetValue(new Position(row, column));

        return values;
    }
}
