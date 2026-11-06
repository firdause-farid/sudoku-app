using Sudoku.Core.Domain.Model;
using Sudoku.Core.Domain.Randomness;

namespace Sudoku.Core.Domain.Generation;

/// <summary>
/// Generates complete Sudoku solution grids using randomized backtracking.
/// </summary>
public sealed class SudokuSolutionGenerator : ISudokuSolutionGenerator
{
    public int[,] GenerateSolution(IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(random);

        var grid = new int?[SudokuRules.GridSize, SudokuRules.GridSize];
        if (!Fill(grid, random))
            throw new InvalidOperationException("Could not generate a complete Sudoku solution.");

        var solution = new int[SudokuRules.GridSize, SudokuRules.GridSize];
        for (int row = 0; row < SudokuRules.GridSize; row++)
            for (int column = 0; column < SudokuRules.GridSize; column++)
                solution[row, column] = grid[row, column]!.Value;

        return solution;
    }

    private static bool Fill(int?[,] grid, IRandomSource random)
    {
        if (!SudokuGridSearch.TryFindEmpty(grid, out int row, out int column))
            return true;

        var numbers = Enumerable.Range(SudokuRules.MinimumValue, SudokuRules.MaximumValue).ToList();
        random.Shuffle(numbers);
        foreach (int number in numbers)
        {
            if (SudokuConstraintChecker.HasPlacementConflict(grid, row, column, number))
                continue;

            grid[row, column] = number;
            if (Fill(grid, random))
                return true;
            grid[row, column] = null;
        }

        return false;
    }
}
