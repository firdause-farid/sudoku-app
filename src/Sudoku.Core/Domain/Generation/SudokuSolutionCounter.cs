using Sudoku.Core.Domain.Model;

namespace Sudoku.Core.Domain.Generation;

/// <summary>
/// Counts Sudoku solutions and checks whether a puzzle has a unique solution.
/// </summary>
public sealed class SudokuSolutionCounter : ISudokuSolutionCounter
{
    private const int UniquenessSolutionLimit = 2;
    private const int UniqueSolutionCount = 1;

    public bool HasUniqueSolution(int?[,] grid)
    {
        return CountSolutions(grid, UniquenessSolutionLimit) == UniqueSolutionCount;
    }

    public int CountSolutions(int?[,] grid, int limit = UniquenessSolutionLimit)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit);
        if (!SudokuConstraintChecker.IsValidGrid(grid))
            return 0;

        return Count((int?[,])grid.Clone(), limit);
    }

    private static int Count(int?[,] grid, int limit)
    {
        if (!SudokuGridSearch.TryFindEmpty(grid, out int row, out int column))
            return UniqueSolutionCount;

        int solutionCount = 0;
        for (int value = SudokuRules.MinimumValue; value <= SudokuRules.MaximumValue; value++)
        {
            if (SudokuConstraintChecker.HasPlacementConflict(grid, row, column, value))
                continue;

            grid[row, column] = value;
            solutionCount += Count(grid, limit);
            grid[row, column] = null;
            if (solutionCount >= limit)
                return solutionCount;
        }

        return solutionCount;
    }
}
