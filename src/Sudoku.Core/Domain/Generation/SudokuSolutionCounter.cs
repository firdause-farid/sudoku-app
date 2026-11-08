using Sudoku.Core.Domain.Model;

namespace Sudoku.Core.Domain.Generation;

/// <summary>
/// Counts Sudoku solutions and checks whether a puzzle has a unique solution.
/// </summary>
public sealed class SudokuSolutionCounter : ISudokuSolutionCounter
{
    private const int UniquenessSolutionLimit = 2;
    private const int UniqueSolutionCount = 1;
    private const int CompletedGridSolutionCount = 1;

    public bool HasUniqueSolution(int?[,] grid, SudokuDefinition definition) =>
        CountSolutions(grid, definition, UniquenessSolutionLimit) == UniqueSolutionCount;

    public int CountSolutions(int?[,] grid, SudokuDefinition definition, int limit = UniquenessSolutionLimit)
    {
        ArgumentNullException.ThrowIfNull(grid);
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(limit);
        if (!SudokuConstraintChecker.IsValidGrid(grid, definition))
            return 0;

        return Count((int?[,])grid.Clone(), limit, definition);
    }

    private static int Count(int?[,] grid, int limit, SudokuDefinition definition)
    {
        if (!SudokuGridSearch.TryFindEmpty(grid, out int row, out int column))
            return CompletedGridSolutionCount;

        int solutionCount = 0;
        for (int value = 1; value <= definition.GridSize; value++)
        {
            if (SudokuConstraintChecker.HasPlacementConflict(grid, row, column, value, definition))
                continue;

            grid[row, column] = value;
            solutionCount += Count(grid, limit, definition);
            grid[row, column] = null;
            if (solutionCount >= limit)
                return solutionCount;
        }

        return solutionCount;
    }
}
