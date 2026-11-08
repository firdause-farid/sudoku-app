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

        if (!SudokuConstraintChecker.TryFindFirstViolation(grid.ToValueArray(), grid.Definition, out ConstraintViolation violation))
            return ValidationResult.Valid();

        return ValidationResult.Invalid(ToIssue(violation));
    }

    private static ValidationIssue ToIssue(ConstraintViolation violation) => violation.Unit switch
    {
        ConstraintUnit.Row => new ValidationIssue(ValidationIssueType.DuplicateRow, violation.Value, violation.UnitIndex),
        ConstraintUnit.Column => new ValidationIssue(ValidationIssueType.DuplicateColumn, violation.Value, violation.UnitIndex),
        ConstraintUnit.Subgrid => new ValidationIssue(ValidationIssueType.DuplicateSubgrid, violation.Value),
        _ => throw new ArgumentOutOfRangeException(nameof(violation), violation.Unit, "Unknown constraint unit.")
    };
}
