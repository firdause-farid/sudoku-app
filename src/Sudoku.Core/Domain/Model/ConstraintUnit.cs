namespace Sudoku.Core.Domain.Model;

/// <summary>The kind of group of cells that must not contain the same value twice.</summary>
internal enum ConstraintUnit
{
    Row,
    Column,
    Subgrid
}
