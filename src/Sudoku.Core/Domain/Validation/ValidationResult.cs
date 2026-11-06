namespace Sudoku.Core.Domain.Validation;

/// <summary>
/// Result of a validation check.
/// </summary>
public sealed class ValidationResult
{
    private ValidationResult(ValidationIssue? issue) => Issue = issue;

    public bool IsValid => Issue is null;
    public ValidationIssue? Issue { get; }

    public static ValidationResult Valid() => new(null);

    public static ValidationResult Invalid(ValidationIssue issue)
    {
        ArgumentNullException.ThrowIfNull(issue);
        return new ValidationResult(issue);
    }
}
