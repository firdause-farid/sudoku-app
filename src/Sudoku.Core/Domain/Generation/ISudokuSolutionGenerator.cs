using Sudoku.Core.Domain.Randomness;

namespace Sudoku.Core.Domain.Generation;

/// <summary>
/// Creates complete, valid Sudoku solution grids.
/// </summary>
public interface ISudokuSolutionGenerator
{
    int[,] GenerateSolution(IRandomSource random);
}
