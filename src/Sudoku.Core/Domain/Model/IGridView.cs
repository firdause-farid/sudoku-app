namespace Sudoku.Core.Domain.Model;

/// <summary>
/// Read-only access to the values and prefilled state of a Sudoku grid.
/// </summary>
public interface IGridView
{
    int? GetValue(Position position);
    bool IsPrefilled(Position position);
    bool IsComplete();
}
