namespace Sudoku.Core.Domain.Model;

/// <summary>A value that appears twice in one row, column or subgrid.</summary>
/// <param name="Unit">Which kind of group contains the duplicate.</param>
/// <param name="UnitIndex">Zero-based index of that row, column or subgrid (subgrids are numbered left to right, then top to bottom).</param>
/// <param name="Value">The duplicated value.</param>
internal readonly record struct ConstraintViolation(ConstraintUnit Unit, int UnitIndex, int Value);
