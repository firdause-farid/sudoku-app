using Sudoku.Core.Domain.Model;
using Sudoku.Core.Domain.Game;
using Sudoku.Core.Domain.Game.Results;
using Sudoku.Core.Domain.Generation;
using Sudoku.Core.Domain.Validation;
using Sudoku.Tests.Fakes;

namespace Sudoku.Tests.Unit.Game;

[Trait("Category", "Unit")]
public class SudokuGameTests
{
    private static (SudokuGame game, Grid puzzle) CreateGame(int seed = 1)
    {
        var (puzzle, solution) = new PuzzleGenerator(
            new SeededRandomSource(seed), new SudokuSolutionGenerator(), new SudokuSolutionCounter()).Generate(30, SudokuDefinition.Classic);
        var game = new SudokuGame(puzzle, solution, new SudokuValidator(), new SeededRandomSource(seed));
        return (game, puzzle);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    public void Place_NumberOutsideRange_ReturnsInvalid(int number)
    {
        var (game, _) = CreateGame();
        var empty = FindEmpty(game.GetGridView());
        var result = game.Place(empty, number);

        Assert.False(result.Accepted);
        Assert.Equal(MoveStatus.InvalidNumber, result.Status);
        Assert.Null(game.GetGridView().GetValue(empty));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(9)]
    public void Place_NumberAtRangeBoundary_IsAccepted(int number)
    {
        var (game, _) = CreateGame();
        var empty = FindEmpty(game.GetGridView());

        var result = game.Place(empty, number);

        Assert.True(result.Accepted);
        Assert.Equal(MoveStatus.Accepted, result.Status);
        Assert.Equal(number, game.GetGridView().GetValue(empty));
    }

    [Fact]
    public void Place_OnPrefilledCell_ReturnsPrefilledCellStatus()
    {
        var (game, puzzle) = CreateGame();
        var prefilledPosition = FindPrefilled(puzzle);
        var result = game.Place(prefilledPosition, 9);
        Assert.False(result.Accepted);
        Assert.Equal(MoveStatus.PrefilledCell, result.Status);
    }

    [Fact]
    public void Place_ValidMove_ReturnsAccepted()
    {
        var (game, _) = CreateGame();
        var empty = FindEmpty(game.GetGridView());
        var result = game.Place(empty, 5);
        Assert.True(result.Accepted);
        Assert.Equal(5, game.GetGridView().GetValue(empty));
    }

    [Fact]
    public void Clear_OnPrefilledCell_ReturnsPrefilledCellStatus()
    {
        var (game, puzzle) = CreateGame();
        var prefilledPosition = FindPrefilled(puzzle);
        var result = game.Clear(prefilledPosition);
        Assert.False(result.Accepted);
        Assert.Equal(MoveStatus.PrefilledCell, result.Status);
    }

    [Fact]
    public void Clear_UserFilled_ClearsCell()
    {
        var (game, _) = CreateGame();
        var empty = FindEmpty(game.GetGridView());
        game.Place(empty, 5);
        var result = game.Clear(empty);
        Assert.True(result.Accepted);
        Assert.Null(game.GetGridView().GetValue(empty));
    }

    [Fact]
    public void Clear_EmptyCell_IsAcceptedAndLeavesCellEmpty()
    {
        var (game, _) = CreateGame();
        var empty = FindEmpty(game.GetGridView());

        var result = game.Clear(empty);

        Assert.True(result.Accepted);
        Assert.Equal(MoveStatus.Accepted, result.Status);
        Assert.Null(game.GetGridView().GetValue(empty));
    }

    [Fact]
    public void Place_UserFilledCell_ReplacesPreviousValue()
    {
        var (game, _) = CreateGame();
        var empty = FindEmpty(game.GetGridView());
        game.Place(empty, 5);

        var result = game.Place(empty, 6);

        Assert.True(result.Accepted);
        Assert.Equal(6, game.GetGridView().GetValue(empty));
    }

    [Fact]
    public void Hint_ReturnsHintForEmptyCell()
    {
        var (game, _) = CreateGame();
        var hint = game.Hint();
        Assert.NotNull(hint);
        Assert.Null(game.GetGridView().GetValue(hint!.Position));
        Assert.InRange(hint.Value, 1, 9);
    }

    [Fact]
    public void Hint_WhenBoardIsFull_ReturnsNull()
    {
        var (puzzle, solution) = CreatePuzzleWithOneEmptyCell();
        var game = new SudokuGame(puzzle, solution, new SudokuValidator(), new SeededRandomSource(1));
        game.Place(new Position(0, 2), solution[0, 2]);

        Assert.Null(game.Hint());
    }

    [Fact]
    public void Check_FreshPuzzle_NoViolations()
    {
        var (game, _) = CreateGame();
        var result = game.Check();
        Assert.True(result.IsValid);
        Assert.Null(result.Issue);
    }

    [Fact]
    public void Check_WithRowDuplicate_ReturnsValidationIssue()
    {
        var (puzzle, solution) = CreatePuzzleWithOneEmptyCell();
        var game = new SudokuGame(puzzle, solution, new SudokuValidator(), new SeededRandomSource(1));
        game.Place(new Position(0, 2), 5); // Duplicates 5 in row A.

        var result = game.Check();

        Assert.False(result.IsValid);
        Assert.Equal(new ValidationIssue(ValidationIssueType.DuplicateRow, 5, 0), result.Issue);
    }

    [Fact]
    public void IsComplete_FalseInitially()
    {
        var (game, _) = CreateGame();
        Assert.False(game.IsComplete());
    }

    [Fact]
    public void IsComplete_TrueWhenCorrectSolutionIsFilled()
    {
        var (puzzle, solution) = CreatePuzzleWithOneEmptyCell();
        var game = new SudokuGame(puzzle, solution, new SudokuValidator(), new SeededRandomSource(1));
        game.Place(new Position(0, 2), solution[0, 2]);

        Assert.True(game.IsComplete());
    }

    [Fact]
    public void IsComplete_FalseWhenGridIsFullButHasRuleViolation()
    {
        var (puzzle, solution) = CreatePuzzleWithOneEmptyCell();
        var game = new SudokuGame(puzzle, solution, new SudokuValidator(), new SeededRandomSource(1));
        game.Place(new Position(0, 2), 5);

        Assert.True(game.GetGridView().IsComplete());
        Assert.False(game.IsComplete());
    }

    [Fact]
    public void Constructor_RejectsSolutionWithWrongDimensions()
    {
        var puzzle = new Grid(SudokuDefinition.Classic);
        var exception = Assert.Throws<ArgumentException>(() =>
            new SudokuGame(puzzle, new int[8, 9], new SudokuValidator(), new SeededRandomSource(1)));

        Assert.Contains("9x9", exception.Message);
    }

    [Fact]
    public void Constructor_RejectsSolutionThatConflictsWithPrefilledCell()
    {
        var (puzzle, solution) = CreatePuzzleWithOneEmptyCell();
        solution[0, 0] = solution[0, 1];

        Assert.Throws<ArgumentException>(() =>
            new SudokuGame(puzzle, solution, new SudokuValidator(), new SeededRandomSource(1)));
    }

    [Fact]
    public void Constructor_RejectsPuzzleWithRuleViolations()
    {
        var (puzzle, solution) = CreatePuzzleWithOneEmptyCell();
        puzzle.SetValue(new Position(0, 2), 5);

        Assert.Throws<ArgumentException>(() =>
            new SudokuGame(puzzle, solution, new SudokuValidator(), new SeededRandomSource(1)));
    }

    [Fact]
    public void Constructor_RejectsSolutionWithRuleViolations()
    {
        var puzzle = new Grid(SudokuDefinition.Classic);
        var (_, solution) = CreatePuzzleWithOneEmptyCell();
        solution[0, 0] = solution[0, 1];

        Assert.Throws<ArgumentException>(() =>
            new SudokuGame(puzzle, solution, new SudokuValidator(), new SeededRandomSource(1)));
    }

    [Fact]
    public void Hint_UsesDefensiveCopyOfSolution()
    {
        var (puzzle, solution) = CreatePuzzleWithOneEmptyCell();
        var game = new SudokuGame(puzzle, solution, new SudokuValidator(), new SeededRandomSource(1));
        solution[0, 2] = 8;

        var hint = game.Hint();

        Assert.Equal(new Position(0, 2), hint!.Position);
        Assert.Equal(4, hint.Value);
    }

    [Fact]
    public void GridView_IsReadOnlyAndReflectsGameChanges()
    {
        var (puzzle, solution) = CreatePuzzleWithOneEmptyCell();
        var game = new SudokuGame(puzzle, solution, new SudokuValidator(), new SeededRandomSource(1));
        var gridView = game.GetGridView();
        var emptyCell = new Position(0, 2);

        Assert.IsNotType<Grid>(gridView);
        Assert.Null(gridView.GetValue(emptyCell));

        game.Place(emptyCell, 8);

        Assert.Equal(8, gridView.GetValue(emptyCell));
    }

    [Fact]
    public void Constructor_CopiesPuzzleSoCallerChangesDoNotAffectGame()
    {
        var (puzzle, solution) = CreatePuzzleWithOneEmptyCell();
        var game = new SudokuGame(puzzle, solution, new SudokuValidator(), new SeededRandomSource(1));
        var emptyCell = new Position(0, 2);

        puzzle.SetValue(emptyCell, 8);

        Assert.Null(game.GetGridView().GetValue(emptyCell));
    }

    [Fact]
    public void Hint_ReturnsSolutionValueForEmptyCell()
    {
        var (puzzle, solution) = CreatePuzzleWithOneEmptyCell();
        var game = new SudokuGame(puzzle, solution, new SudokuValidator(), new SeededRandomSource(1));

        var hint = game.Hint();

        Assert.Equal(new HintResult(new Position(0, 2), solution[0, 2]), hint);
    }

    private static (Grid Puzzle, int[,] Solution) CreatePuzzleWithOneEmptyCell()
    {
        int[,] solution =
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
        var values = new int?[9, 9];
        var prefilledMask = new bool[9, 9];
        for (int row = 0; row < 9; row++)
            for (int column = 0; column < 9; column++)
                if (row != 0 || column != 2)
                {
                    values[row, column] = solution[row, column];
                    prefilledMask[row, column] = true;
                }

        return (Grid.FromArrays(values, prefilledMask, SudokuDefinition.Classic), solution);
    }

    private static Position FindEmpty(IGridView grid)
    {
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                if (grid.GetValue(new Position(r, c)) is null)
                    return new Position(r, c);
        throw new InvalidOperationException("No empty cell.");
    }

    private static Position FindPrefilled(Grid grid)
    {
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                if (grid.IsPrefilled(new Position(r, c)))
                    return new Position(r, c);
        throw new InvalidOperationException("No prefilled cell.");
    }
}
