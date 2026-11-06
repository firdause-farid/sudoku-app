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
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
        if ((type is ValidationIssueType.DuplicateRow or ValidationIssueType.DuplicateColumn) && (unitIndex is null or < 0))
            throw new ArgumentOutOfRangeException(nameof(unitIndex));
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
