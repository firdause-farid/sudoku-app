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
        return HasUniqueSolution(grid, SudokuDefinition.Classic);
    }

    public bool HasUniqueSolution(int?[,] grid, SudokuDefinition definition) =>
        CountSolutions(grid, UniquenessSolutionLimit, definition) == UniqueSolutionCount;

    public int CountSolutions(int?[,] grid, int limit = UniquenessSolutionLimit)
        => CountSolutions(grid, limit, SudokuDefinition.Classic);

    public int CountSolutions(int?[,] grid, int limit, SudokuDefinition definition)
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
            return UniqueSolutionCount;

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
