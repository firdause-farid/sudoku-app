using Sudoku.Core.Domain.Model;

namespace Sudoku.Core.Domain.Validation;

/// <summary>
/// Identifies a Sudoku rule violation without choosing how it is presented.
/// </summary>
public sealed record ValidationIssue
{
    public ValidationIssue(ValidationIssueType type, int value, int? unitIndex = null)
    {
        if (!Enum.IsDefined(type))
            throw new ArgumentOutOfRangeException(nameof(type));
        if (value is < SudokuRules.MinimumValue or > SudokuRules.MaximumValue)
            throw new ArgumentOutOfRangeException(nameof(value), $"Issue value must be between {SudokuRules.MinimumValue} and {SudokuRules.MaximumValue}.");
        if ((type is ValidationIssueType.DuplicateRow or ValidationIssueType.DuplicateColumn) && unitIndex is not (>= 0 and < SudokuRules.GridSize))
            throw new ArgumentOutOfRangeException(nameof(unitIndex), $"Row and column issues require a unit index between 0 and {SudokuRules.GridSize - 1}.");
        if (type == ValidationIssueType.DuplicateSubgrid && unitIndex is not null)
            throw new ArgumentException("Subgrid issues do not have a row or column unit index.", nameof(unitIndex));

        Type = type;
        Value = value;
        UnitIndex = unitIndex;
    }

    public ValidationIssueType Type { get; }
    public int Value { get; }
    public int? UnitIndex { get; }
}
