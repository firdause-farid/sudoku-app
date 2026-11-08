using Sudoku.Core.Application.Commands;
using Sudoku.Core.Application.Ports;
using Sudoku.Core.Domain.Game;
using Sudoku.Core.Presentation.Messages;

namespace Sudoku.Core.Application.Runtime;

/// <summary>
/// Runs the console session and manages game creation and replay.
/// </summary>
public sealed class ConsoleGameRunner
{
    private readonly IConsole _console;
    private readonly ISudokuGameFactory _gameFactory;
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly IExceptionLogger _exceptionLogger;

    public ConsoleGameRunner(
        IConsole console,
        ISudokuGameFactory gameFactory,
        ICommandDispatcher commandDispatcher,
        IExceptionLogger exceptionLogger)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _gameFactory = gameFactory ?? throw new ArgumentNullException(nameof(gameFactory));
        _commandDispatcher = commandDispatcher ?? throw new ArgumentNullException(nameof(commandDispatcher));
        _exceptionLogger = exceptionLogger ?? throw new ArgumentNullException(nameof(exceptionLogger));
    }

    public void Run()
    {
        ISudokuGame? game = TryStartNewGame();

        while (game is not null)
        {
            _console.WriteLine(UserMessages.CommandPrompt);
            string? line = _console.ReadLine();
            if (line is null)
                return;

            game = NextGameAfter(ExecuteSafely(line, game), game);
        }
    }

    /// <summary>Runs one command. An unexpected failure is logged and reported, and play continues.</summary>
    private CommandAction ExecuteSafely(string line, ISudokuGame game)
    {
        try
        {
            return _commandDispatcher.Handle(line, game);
        }
        catch (Exception exception)
        {
            _exceptionLogger.Log(exception, "Handling a player command");
            _console.WriteLine(UserMessages.UnexpectedError);
            return CommandAction.Continue;
        }
    }

    /// <summary>Returns the game to keep playing, a fresh game after a win, or null when the session should end.</summary>
    private ISudokuGame? NextGameAfter(CommandAction action, ISudokuGame game)
    {
        if (action == CommandAction.Quit)
            return null;

        if (action == CommandAction.MoveAccepted && game.IsComplete())
            return CompleteGameAndReplay();

        return game;
    }

    private ISudokuGame? TryStartNewGame()
    {
        ISudokuGame game;
        try
        {
            game = _gameFactory.Create();
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            _exceptionLogger.Log(ex, "Starting a new game");
            _console.WriteLine(UserMessages.UnableToStartGame);
            return null;
        }

        _console.WriteLine(UserMessages.Welcome);
        _console.WriteLine();
        _console.WriteLine(UserMessages.PuzzleHeading);
        _commandDispatcher.PrintGrid(game);
        return game;
    }

    private ISudokuGame? CompleteGameAndReplay()
    {
        _console.WriteLine();
        _console.WriteLine(UserMessages.GameCompleted);
        _console.WriteLine(UserMessages.ReplayPrompt);
        _console.ReadKey(true);
        return TryStartNewGame();
    }
}
