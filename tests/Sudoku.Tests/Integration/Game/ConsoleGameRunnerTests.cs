using System.Text.RegularExpressions;
using Sudoku.Core.Application.Commands;
using Sudoku.Core.Application.Commands.Handlers;
using Sudoku.Core.Application.Ports;
using Sudoku.Core.Domain.Model;
using Sudoku.Core.Domain.Game;
using Sudoku.Core.Domain.Game.Results;
using Sudoku.Core.Application.Runtime;
using Sudoku.Core.Domain.Generation;
using Sudoku.Core.Domain.Validation;
using Sudoku.Core.Presentation.Messages;
using Sudoku.Core.Presentation.Rendering;
using Sudoku.Core.Presentation.Rendering.Styles;
using Sudoku.Tests.Fakes;

namespace Sudoku.Tests.Integration.Game;

[Trait("Category", "Integration")]
public class ConsoleGameRunnerTests
{
    [Fact]
    public void Quit_PrintsThanksAndExits()
    {
        var console = new FakeConsole();
        console.Inputs.Enqueue("quit");

        var engine = CreateRunner(console, seed: 1);
        bool endedNormally = engine.Run();

        Assert.True(endedNormally);
        Assert.Contains(UserMessages.ThanksForPlaying, string.Join("\n", console.Outputs));
    }

    [Fact]
    public void Run_WhenInputEnds_EndsNormally()
    {
        var console = new FakeConsole();

        bool endedNormally = CreateRunner(console, seed: 1).Run();

        Assert.True(endedNormally);
    }

    [Fact]
    public void Hint_PrintsExpectedFormatAndDoesNotPrintMoveAccepted()
    {
        var console = new FakeConsole();
        console.Inputs.Enqueue("hint");
        console.Inputs.Enqueue("quit");

        var engine = CreateRunner(console, seed: 2);
        engine.Run();

        string hintTemplate = UserMessages.Hint("__POSITION__", "999");
        string hintPattern = "^" + Regex.Escape(hintTemplate)
            .Replace(Regex.Escape("__POSITION__"), @"(?<position>[A-I][1-9])")
            .Replace("999", @"(?<value>[1-9])") + "$";
        var hintLine = console.Outputs.FirstOrDefault(o => Regex.IsMatch(o, hintPattern));
        Assert.NotNull(hintLine);
        var hintMatch = Regex.Match(hintLine!, hintPattern);
        Assert.True(hintMatch.Success);
        Assert.Equal(
            UserMessages.Hint(hintMatch.Groups["position"].Value, hintMatch.Groups["value"].Value),
            hintLine);
        Assert.DoesNotContain(UserMessages.MoveAccepted, console.Outputs);
    }

    [Fact]
    public void Place_OnPrefilledCell_PrintsPrefilledMessage()
    {
        const int seed = 7;
        var puzzle = CreateGenerator(seed).Generate(30, SudokuDefinition.Classic).Puzzle;

        Position? prefilledPosition = null;
        for (int r = 0; r < 9 && prefilledPosition is null; r++)
            for (int c = 0; c < 9; c++)
                if (puzzle.GetValue(new Position(r, c)).HasValue)
                {
                    prefilledPosition = new Position(r, c);
                    break;
                }

        Assert.NotNull(prefilledPosition);

        var console = new FakeConsole();
        console.Inputs.Enqueue($"{Format(prefilledPosition)} 6");
        console.Inputs.Enqueue("quit");

        var engine = CreateRunner(console, seed: seed);
        engine.Run();

        var all = string.Join("\n", console.Outputs);
        Assert.Contains(UserMessages.PrefilledCell(Format(prefilledPosition)), all);
    }

    [Fact]
    public void Place_NumberOutsideRange_PrintsInvalidNumberMessage()
    {
        var console = new FakeConsole();
        console.Inputs.Enqueue("A1 10");
        console.Inputs.Enqueue("quit");
        var runner = CreateRunner(console, seed: 15);

        runner.Run();

        Assert.Contains(UserMessages.InvalidMoveNumberFor(9), console.Outputs);
    }

    [Fact]
    public void Check_OnFreshPuzzle_PrintsNoViolations()
    {
        var console = new FakeConsole();
        console.Inputs.Enqueue("check");
        console.Inputs.Enqueue("quit");

        var engine = CreateRunner(console, seed: 3);
        engine.Run();

        Assert.Contains(UserMessages.NoRuleViolations, string.Join("\n", console.Outputs));
    }

    [Fact]
    public void Run_UsesInjectedRenderer()
    {
        var console = new FakeConsole();
        console.Inputs.Enqueue("quit");
        var renderer = new StubRenderer();
        var engine = CreateRunner(console, seed: 14, renderer: renderer);

        engine.Run();

        Assert.True(renderer.WasCalled);
        Assert.Contains("rendered by test renderer", console.Outputs);
    }

    [Fact]
    public void Run_UsesInjectedGameFactory()
    {
        var console = new FakeConsole();
        console.Inputs.Enqueue("quit");
        var gameFactory = new RecordingGameFactory();

        CreateRunner(console, gameFactory: gameFactory).Run();

        Assert.True(gameFactory.WasCalled);
    }

    [Fact]
    public void Run_WhenCommandHandlerThrows_LogsErrorAndContinuesPlaying()
    {
        var console = new FakeConsole();
        console.Inputs.Enqueue("hint");
        console.Inputs.Enqueue("quit");
        var logger = new RecordingExceptionLogger();
        var commandDispatcher = new ThrowOnceDispatcher();
        var runner = new ConsoleGameRunner(
            console,
            new RecordingGameFactory(),
            commandDispatcher,
            logger);

        runner.Run();

        Assert.Contains(UserMessages.UnexpectedError, console.Outputs);
        Assert.Single(logger.Exceptions);
        Assert.Equal("Handling a player command", logger.Exceptions[0].Context);
        Assert.Equal(2, commandDispatcher.HandleCount);
    }

    [Fact]
    public void Run_WhenInitialPuzzleGenerationFails_PrintsErrorAndExits()
    {
        var console = new FakeConsole();
        var logger = new RecordingExceptionLogger();
        var engine = CreateRunner(console, generator: new FailingPuzzleGenerator(), exceptionLogger: logger);

        bool endedNormally = engine.Run();

        Assert.False(endedNormally);
        Assert.Contains(UserMessages.UnableToStartGame, console.Outputs);
        Assert.DoesNotContain(UserMessages.PuzzleHeading, console.Outputs);
        Assert.Single(logger.Exceptions);
        Assert.Contains("test generation failure", logger.Exceptions[0].Exception.Message);
    }

    [Fact]
    public void Run_WhenGeneratorReturnsInvalidPuzzle_PrintsErrorAndExits()
    {
        var console = new FakeConsole();
        var logger = new RecordingExceptionLogger();
        var engine = CreateRunner(console, generator: new InvalidPuzzleGenerator(), exceptionLogger: logger);

        bool endedNormally = engine.Run();

        Assert.False(endedNormally);
        Assert.Contains(UserMessages.UnableToStartGame, console.Outputs);
        Assert.DoesNotContain(UserMessages.PuzzleHeading, console.Outputs);
        Assert.Single(logger.Exceptions);
        Assert.Contains("Solution values must be between 1 and 9", logger.Exceptions[0].Exception.Message);
    }

    [Fact]
    public void Playthrough_WhenNextPuzzleGenerationFails_PrintsErrorAndExits()
    {
        const int seed = 43;
        var (puzzle, solution) = CreateGenerator(seed).Generate(30, SudokuDefinition.Classic);
        var console = new FakeConsole();
        var logger = new RecordingExceptionLogger();
        for (int row = 0; row < 9; row++)
            for (int column = 0; column < 9; column++)
                if (!puzzle.GetValue(new Position(row, column)).HasValue)
                    console.Inputs.Enqueue($"{(char)('A' + row)}{column + 1} {solution[row, column]}");

        var engine = CreateRunner(
            console,
            seed: seed,
            generator: new FailAfterFirstPuzzleGenerator(CreateGenerator(seed)),
            exceptionLogger: logger);

        bool endedNormally = engine.Run();

        Assert.False(endedNormally);
        Assert.Contains(UserMessages.GameCompleted, console.Outputs);
        Assert.Contains(UserMessages.UnableToStartGame, console.Outputs);
        Assert.Single(logger.Exceptions);
    }

    [Fact]
    public void Clear_OnUserFilledCell_PrintsAcceptedFeedback()
    {
        const int seed = 11;
        var puzzle = CreateGenerator(seed).Generate(30, SudokuDefinition.Classic).Puzzle;

        Position? emptyPos = null;
        for (int r = 0; r < 9 && emptyPos is null; r++)
            for (int c = 0; c < 9; c++)
                if (!puzzle.GetValue(new Position(r, c)).HasValue)
                {
                    emptyPos = new Position(r, c);
                    break;
                }

        Assert.NotNull(emptyPos);

        var console = new FakeConsole();
        console.Inputs.Enqueue($"{Format(emptyPos)} 5");
        console.Inputs.Enqueue($"{Format(emptyPos)} clear");
        console.Inputs.Enqueue("quit");

        var engine = CreateRunner(console, seed: seed);
        engine.Run();

        var moveAcceptedCount = console.Outputs.Count(o => o == UserMessages.MoveAccepted);
        Assert.Equal(2, moveAcceptedCount);
    }

    [Fact]
    public void Clear_OnPrefilledCell_PrintsPrefilledMessage()
    {
        const int seed = 12;
        var puzzle = CreateGenerator(seed).Generate(30, SudokuDefinition.Classic).Puzzle;

        Position? prefilledPosition = null;
        for (int r = 0; r < 9 && prefilledPosition is null; r++)
            for (int c = 0; c < 9; c++)
                if (puzzle.GetValue(new Position(r, c)).HasValue)
                {
                    prefilledPosition = new Position(r, c);
                    break;
                }

        Assert.NotNull(prefilledPosition);

        var console = new FakeConsole();
        console.Inputs.Enqueue($"{Format(prefilledPosition)} clear");
        console.Inputs.Enqueue("quit");

        var engine = CreateRunner(console, seed: seed);
        engine.Run();

        var all = string.Join("\n", console.Outputs);
        Assert.Contains(UserMessages.PrefilledCell(Format(prefilledPosition)), all);
    }

    [Fact]
    public void InvalidCommand_PrintsInvalidCommand()
    {
        var console = new FakeConsole();
        console.Inputs.Enqueue("foo bar");
        console.Inputs.Enqueue("quit");

        var engine = CreateRunner(console, seed: 4);
        engine.Run();

        var all = string.Join("\n", console.Outputs);
        Assert.Contains(UserMessages.InvalidCommand(UserMessages.UnrecognizedCommand), all);
    }

    [Theory]
    [InlineData("J1 5", "row")]
    [InlineData("A0 5", "column")]
    [InlineData("AAA 5", "format")]
    public void InvalidPosition_PrintsSpecificFeedback(string input, string errorKind)
    {
        var console = new FakeConsole();
        console.Inputs.Enqueue(input);
        console.Inputs.Enqueue("quit");
        var runner = CreateRunner(console, seed: 16);
        string message = errorKind switch
        {
            "row" => UserMessages.RowRangeFor("A", "I"),
            "column" => UserMessages.ColumnRangeFor(9),
            "format" => UserMessages.PositionFormat,
            _ => throw new ArgumentOutOfRangeException(nameof(errorKind), errorKind, null)
        };
        runner.Run();

        Assert.Contains(UserMessages.InvalidCommand(message), console.Outputs);
    }

    [Fact]
    public void Run_WhenInputEnds_ExitsWithoutThanksMessage()
    {
        var console = new FakeConsole();
        var runner = CreateRunner(console, seed: 17);

        runner.Run();

        Assert.Contains(UserMessages.PuzzleHeading, console.Outputs);
        Assert.DoesNotContain(UserMessages.ThanksForPlaying, console.Outputs);
    }

    [Fact]
    public void Check_WithRowDuplicate_PrintsViolation()
    {
        const int seed = 13;
        var puzzle = CreateGenerator(seed).Generate(30, SudokuDefinition.Classic).Puzzle;

        // Find an empty cell and a prefilled value in the same row to create a duplicate.
        Position? emptyPos = null;
        int duplicateValue = 0;
        for (int r = 0; r < 9 && emptyPos is null; r++)
        {
            int? prefilledInRow = null;
            for (int c = 0; c < 9; c++)
            {
                var v = puzzle.GetValue(new Position(r, c));
                if (v.HasValue) { prefilledInRow = v.Value; break; }
            }

            if (prefilledInRow.HasValue)
            {
                for (int c = 0; c < 9; c++)
                {
                    if (!puzzle.GetValue(new Position(r, c)).HasValue)
                    {
                        emptyPos = new Position(r, c);
                        duplicateValue = prefilledInRow.Value;
                        break;
                    }
                }
            }
        }

        Assert.NotNull(emptyPos);

        var console = new FakeConsole();
        console.Inputs.Enqueue($"{Format(emptyPos)} {duplicateValue}");
        console.Inputs.Enqueue("check");
        console.Inputs.Enqueue("quit");

        var engine = CreateRunner(console, seed: seed);
        engine.Run();

        var all = string.Join("\n", console.Outputs);
        Assert.Contains(UserMessages.DuplicateRow(duplicateValue.ToString(), SudokuDefinition.Classic.RowLabels[emptyPos.Value.Row]), all);
    }

    [Fact]
    public void Check_WithColumnDuplicate_PrintsViolation()
    {
        var duplicatePosition = new Position(1, 0);
        var targetPosition = new Position(0, 0);
        var (puzzle, solution) = CreatePuzzleWithSinglePrefilledCell(duplicatePosition);
        int duplicateValue = solution[duplicatePosition.Row, duplicatePosition.Column];
        var console = new FakeConsole();
        console.Inputs.Enqueue($"{Format(targetPosition)} {duplicateValue}");
        console.Inputs.Enqueue("check");
        console.Inputs.Enqueue("quit");
        var runner = CreateRunner(console, generator: new SinglePuzzleGenerator(puzzle, solution));

        runner.Run();

        Assert.Contains(UserMessages.DuplicateColumn(duplicateValue.ToString(), targetPosition.Column + 1), console.Outputs);
    }

    [Fact]
    public void Check_WithSubgridDuplicate_PrintsViolation()
    {
        var duplicatePosition = new Position(1, 1);
        var targetPosition = new Position(0, 0);
        var (puzzle, solution) = CreatePuzzleWithSinglePrefilledCell(duplicatePosition);
        int duplicateValue = solution[duplicatePosition.Row, duplicatePosition.Column];
        var console = new FakeConsole();
        console.Inputs.Enqueue($"{Format(targetPosition)} {duplicateValue}");
        console.Inputs.Enqueue("check");
        console.Inputs.Enqueue("quit");
        var runner = CreateRunner(console, generator: new SinglePuzzleGenerator(puzzle, solution));

        runner.Run();

        Assert.Contains(UserMessages.DuplicateSubgrid(duplicateValue.ToString(), 3, 3), console.Outputs);
    }

    private static string Format(Position? position) =>
        SudokuDefinition.Classic.FormatPosition(position!.Value);

    private static (Grid Puzzle, int[,] Solution) CreatePuzzleWithSinglePrefilledCell(Position prefilledPosition)
    {
        var (_, solution) = CreateGenerator(18).Generate(30, SudokuDefinition.Classic);
        var values = new int?[9, 9];
        var prefilledMask = new bool[9, 9];
        values[prefilledPosition.Row, prefilledPosition.Column] =
            solution[prefilledPosition.Row, prefilledPosition.Column];
        prefilledMask[prefilledPosition.Row, prefilledPosition.Column] = true;

        return (Grid.FromArrays(values, prefilledMask, SudokuDefinition.Classic), solution);
    }

    private static IPuzzleGenerator CreateGenerator(int seed)
    {
        var random = new SeededRandomSource(seed);
        return new PuzzleGenerator(random, new SudokuSolutionGenerator(), new SudokuSolutionCounter());
    }

    private static ConsoleGameRunner CreateRunner(
        FakeConsole console,
        int seed = 1,
        IPuzzleGenerator? generator = null,
        IRenderer? renderer = null,
        IExceptionLogger? exceptionLogger = null,
        ISudokuGameFactory? gameFactory = null)
    {
        var random = new SeededRandomSource(seed);
        IRenderer actualRenderer = renderer ?? new GridRenderer(new PlainGridStyle());
        IPuzzleGenerator actualGenerator = generator ?? new PuzzleGenerator(
            random,
            new SudokuSolutionGenerator(),
            new SudokuSolutionCounter());
        return new ConsoleGameRunner(
            console,
            gameFactory ?? new SudokuGameFactory(actualGenerator, new SudokuValidator(), random, SudokuDefinition.Classic),
            new CommandDispatcher(console, actualRenderer, new ICommandHandler[]
            {
                new HintCommandHandler(console), new CheckCommandHandler(console), new QuitCommandHandler(console),
                new PlaceCommandHandler(console, actualRenderer), new ClearCommandHandler(console, actualRenderer)
            }),
            exceptionLogger ?? new NullExceptionLogger());
    }

    private sealed class StubRenderer : IRenderer
    {
        public bool WasCalled { get; private set; }

        public string Render(IGridView grid)
        {
            WasCalled = true;
            return "rendered by test renderer";
        }
    }

    private sealed class ThrowOnceDispatcher : ICommandDispatcher
    {
        public int HandleCount { get; private set; }

        public CommandAction Handle(string input, ISudokuGame game)
        {
            HandleCount++;
            if (HandleCount == 1)
                throw new InvalidOperationException("test command failure");

            return CommandAction.Quit;
        }

        public void PrintGrid(ISudokuGame game)
        {
        }
    }

    private sealed class RecordingGameFactory : ISudokuGameFactory
    {
        public bool WasCalled { get; private set; }

        public ISudokuGame Create()
        {
            WasCalled = true;
            return new StubSudokuGame();
        }
    }

    private sealed class StubSudokuGame : ISudokuGame
    {
        private readonly Grid _grid = new(SudokuDefinition.Classic);

        public IGridView GetGridView() => _grid;
        public MoveResult Place(Position position, int number) => new(MoveStatus.Accepted);
        public MoveResult Clear(Position position) => new(MoveStatus.Accepted);
        public HintResult? Hint() => null;
        public ValidationResult Check() => ValidationResult.Valid();
        public bool IsComplete() => false;
    }

    private sealed class FailingPuzzleGenerator : IPuzzleGenerator
    {
        public (Grid Puzzle, int[,] Solution) Generate(int prefilledCount, SudokuDefinition definition) =>
            throw new InvalidOperationException("test generation failure");
    }

    private sealed class InvalidPuzzleGenerator : IPuzzleGenerator
    {
        public (Grid Puzzle, int[,] Solution) Generate(int prefilledCount, SudokuDefinition definition) =>
            (new Grid(SudokuDefinition.Classic), new int[9, 9]);
    }

    private sealed class SinglePuzzleGenerator(Grid puzzle, int[,] solution) : IPuzzleGenerator
    {
        public (Grid Puzzle, int[,] Solution) Generate(int prefilledCount, SudokuDefinition definition) =>
            (puzzle, (int[,])solution.Clone());
    }

    private sealed class RecordingExceptionLogger : IExceptionLogger
    {
        public List<(Exception Exception, string Context)> Exceptions { get; } = new();

        public void Log(Exception exception, string context) => Exceptions.Add((exception, context));
    }

    private sealed class FailAfterFirstPuzzleGenerator(IPuzzleGenerator inner) : IPuzzleGenerator
    {
        private bool _hasGenerated;

        public (Grid Puzzle, int[,] Solution) Generate(int prefilledCount, SudokuDefinition definition)
        {
            if (_hasGenerated)
                throw new InvalidOperationException("test generation failure");

            _hasGenerated = true;
            return inner.Generate(prefilledCount, SudokuDefinition.Classic);
        }
    }

}
