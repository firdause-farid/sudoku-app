namespace Sudoku.Core.Domain.Generation;

/// <summary>
/// Counts possible solutions for a Sudoku grid and determines whether its solution is unique.
/// </summary>
public interface ISudokuSolutionCounter
{
    bool HasUniqueSolution(int?[,] grid);
}
