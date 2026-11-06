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
        SudokuDefinition definition = grid.Definition;
        for (int r = 0; r < definition.GridSize; r++)
        {
            if (SudokuConstraintChecker.TryFindDuplicateInRow(values, r, definition, out int duplicate))
                return ValidationResult.Invalid(
                    new ValidationIssue(ValidationIssueType.DuplicateRow, duplicate, r));
        }

        // Columns
        for (int c = 0; c < definition.GridSize; c++)
        {
            if (SudokuConstraintChecker.TryFindDuplicateInColumn(values, c, definition, out int duplicate))
                return ValidationResult.Invalid(
                    new ValidationIssue(ValidationIssueType.DuplicateColumn, duplicate, c));
        }

        // Subgrids
        for (int subgridRowIndex = 0; subgridRowIndex < definition.GridSize / definition.SubgridRows; subgridRowIndex++)
        {
            for (int subgridColumnIndex = 0; subgridColumnIndex < definition.GridSize / definition.SubgridColumns; subgridColumnIndex++)
            {
                int startRow = subgridRowIndex * definition.SubgridRows;
                int startColumn = subgridColumnIndex * definition.SubgridColumns;
                if (SudokuConstraintChecker.TryFindDuplicateInSubgrid(values, startRow, startColumn, definition, out int duplicate))
                    return ValidationResult.Invalid(
                        new ValidationIssue(ValidationIssueType.DuplicateSubgrid, duplicate));
            }
        }

        return ValidationResult.Valid();
    }

    private static int?[,] ToValueArray(IGridView grid)
    {
        var values = new int?[grid.Definition.GridSize, grid.Definition.GridSize];
        for (int row = 0; row < grid.Definition.GridSize; row++)
            for (int column = 0; column < grid.Definition.GridSize; column++)
                values[row, column] = grid.GetValue(new Position(row, column));

        return values;
    }
}
