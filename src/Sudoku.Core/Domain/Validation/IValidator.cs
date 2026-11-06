using Sudoku.Core.Domain.Model;

namespace Sudoku.Core.Domain.Validation;

/// <summary>
/// Validates a Sudoku grid.
/// </summary>
public interface IValidator
{
    ValidationResult Validate(IGridView grid);
}
