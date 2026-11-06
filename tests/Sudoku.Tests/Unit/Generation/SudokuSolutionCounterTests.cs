using Sudoku.Core.Domain.Generation;

namespace Sudoku.Tests.Unit.Generation;

[Trait("Category", "Unit")]
public class SudokuSolutionCounterTests
{
    private readonly SudokuSolutionCounter _counter = new();

    private static readonly int?[,] SolvedGrid =
    {
        { 5, 3, 4, 6, 7, 8, 9, 1, 2 },
        { 6, 7, 2, 1, 9, 5, 3, 4, 8 },
        { 1, 9, 8, 3, 4, 2, 5, 6, 7 },
        { 8, 5, 9, 7, 6, 1, 4, 2, 3 },
        { 4, 2, 6, 8, 5, 3, 7, 9, 1 },
        { 7, 1, 3, 9, 2, 4, 8, 5, 6 },
        { 9, 6, 1, 5, 3, 7, 2, 8, 4 },
        { 2, 8, 7, 4, 1, 9, 6, 3, 5 },
        { 3, 4, 5, 2, 8, 6, 1, 7, 9 }
    };

    [Fact]
    public void CountSolutions_ReturnsOneForUniquePuzzleAndDoesNotMutateInput()
    {
        var puzzle = (int?[,])SolvedGrid.Clone();
        puzzle[0, 0] = null;
        var original = (int?[,])puzzle.Clone();

        int count = _counter.CountSolutions(puzzle);

        Assert.Equal(1, count);
        AssertGridEqual(original, puzzle);
    }

    [Fact]
    public void HasUniqueSolution_ReturnsTrueForUniquePuzzle()
    {
        var puzzle = (int?[,])SolvedGrid.Clone();
        puzzle[0, 0] = null;

        Assert.True(_counter.HasUniqueSolution(puzzle));
    }

    [Fact]
    public void HasUniqueSolution_ReturnsFalseForPuzzleWithMultipleSolutions()
    {
        Assert.False(_counter.HasUniqueSolution(new int?[9, 9]));
    }

    [Fact]
    public void CountSolutions_ReturnsZeroWhenPuzzleCannotBeSolved()
    {
        var grid = new int?[9, 9];
        for (int column = 1; column < 9; column++)
            grid[0, column] = column + 1;
        grid[1, 0] = 1;

        Assert.Equal(0, _counter.CountSolutions(grid));
    }

    [Fact]
    public void CountSolutions_ReturnsZeroForCompletedGridWithRuleViolations()
    {
        var grid = (int?[,])SolvedGrid.Clone();
        grid[0, 0] = grid[0, 1];

        Assert.Equal(0, _counter.CountSolutions(grid));
    }

    [Fact]
    public void CountSolutions_RejectsNonPositiveLimit()
    {
        var grid = (int?[,])SolvedGrid.Clone();

        Assert.Throws<ArgumentOutOfRangeException>(() => _counter.CountSolutions(grid, limit: 0));
    }

    [Fact]
    public void CountSolutions_StopsAtRequestedLimitForAmbiguousPuzzle()
    {
        var emptyGrid = new int?[9, 9];

        Assert.Equal(2, _counter.CountSolutions(emptyGrid, limit: 2));
    }

    private static void AssertGridEqual(int?[,] expected, int?[,] actual)
    {
        for (int row = 0; row < 9; row++)
            for (int column = 0; column < 9; column++)
                Assert.Equal(expected[row, column], actual[row, column]);
    }
}
