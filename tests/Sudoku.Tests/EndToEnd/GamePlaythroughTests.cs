using Sudoku.Core.Domain.Model;
using Sudoku.Core.Application.Runtime;
using Sudoku.Core.Domain.Game;
using Sudoku.Core.Domain.Generation;
using Sudoku.Core.Domain.Validation;
using Sudoku.Core.Presentation.Rendering;
using Sudoku.Core.Presentation.Messages;
using Sudoku.Tests.Fakes;

namespace Sudoku.Tests.EndToEnd;

[Trait("Category", "EndToEnd")]
public class GamePlaythroughTests
{
    [Fact]
    public void CompletingPuzzle_WaitsForKeyAndStartsAnotherGame()
    {
        const int seed = 42;
        var (puzzle, solution) = CreateGenerator(seed).Generate(30);

        var console = new FakeConsole();
        for (int row = 0; row < 9; row++)
            for (int column = 0; column < 9; column++)
                if (!puzzle.GetValue(new Position(row, column)).HasValue)
                    console.Inputs.Enqueue($"{(char)('A' + row)}{column + 1} {solution[row, column]}");

        var random = new SeededRandomSource(seed);
        var generator = new PuzzleGenerator(random, new SudokuSolutionGenerator(), new SudokuSolutionCounter());
        var gameFactory = new SudokuGameFactory(generator, new SudokuValidator(), random);
        var renderer = new GridRenderer(new PlainGridStyle());
        var engine = new ConsoleGameRunner(
            console,
            gameFactory,
            new ConsoleCommandHandler(console, renderer),
            new NullExceptionLogger());
        engine.Run();

        Assert.Contains(UserMessages.GameCompleted, console.Outputs);
        Assert.Contains(UserMessages.ReplayPrompt, console.Outputs);
        Assert.Equal(1, console.ReadKeyCalls);
        Assert.Equal(2, console.Outputs.Count(output => output == UserMessages.PuzzleHeading));
    }

    private static PuzzleGenerator CreateGenerator(int seed) =>
        new(new SeededRandomSource(seed), new SudokuSolutionGenerator(), new SudokuSolutionCounter());
}
