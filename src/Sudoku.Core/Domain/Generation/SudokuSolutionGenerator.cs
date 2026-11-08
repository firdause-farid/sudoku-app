using Sudoku.Core.Domain.Model;
using Sudoku.Core.Domain.Randomness;

namespace Sudoku.Core.Domain.Generation;

/// <summary>
/// Generates complete Sudoku solution grids using randomized backtracking.
/// </summary>
public sealed class SudokuSolutionGenerator : ISudokuSolutionGenerator
{
    public int[,] GenerateSolution(IRandomSource random, SudokuDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(random);
        ArgumentNullException.ThrowIfNull(definition);

        var grid = new int?[definition.GridSize, definition.GridSize];
        if (!Fill(grid, random, definition))
            throw new InvalidOperationException("Could not generate a complete Sudoku solution.");

        var solution = new int[definition.GridSize, definition.GridSize];
        for (int row = 0; row < definition.GridSize; row++)
            for (int column = 0; column < definition.GridSize; column++)
                solution[row, column] = grid[row, column]!.Value;

        return solution;
    }

    private static bool Fill(int?[,] grid, IRandomSource random, SudokuDefinition definition)
    {
        if (!SudokuGridSearch.TryFindEmpty(grid, out int row, out int column))
            return true;

        var numbers = Enumerable.Range(1, definition.GridSize).ToList();
        random.Shuffle(numbers);
        foreach (int number in numbers)
        {
            if (SudokuConstraintChecker.HasPlacementConflict(grid, row, column, number, definition))
                continue;

            grid[row, column] = number;
            if (Fill(grid, random, definition))
                return true;
            grid[row, column] = null;
        }

        return false;
    }
}
