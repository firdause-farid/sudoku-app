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
    private readonly IConsoleCommandHandler _commandHandler;
    private readonly IExceptionLogger _exceptionLogger;
    private ISudokuGame? _game;
    private bool _running;

    public ConsoleGameRunner(
        IConsole console,
        ISudokuGameFactory gameFactory,
        IConsoleCommandHandler commandHandler,
        IExceptionLogger exceptionLogger)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _gameFactory = gameFactory ?? throw new ArgumentNullException(nameof(gameFactory));
        _commandHandler = commandHandler ?? throw new ArgumentNullException(nameof(commandHandler));
        _exceptionLogger = exceptionLogger ?? throw new ArgumentNullException(nameof(exceptionLogger));
    }

    public void Run()
    {
        _running = TryStartNewGame();

        while (_running)
        {
            _console.WriteLine(UserMessages.CommandPrompt);
            string? line = _console.ReadLine();
            if (line is null)
            {
                _running = false;
                break;
            }

            ISudokuGame game = _game ?? throw new InvalidOperationException("A game must be started before reading commands.");
            CommandAction action;
            try
            {
                action = _commandHandler.Handle(line, game);
            }
            catch (Exception exception)
            {
                _exceptionLogger.Log(exception, "Handling a player command");
                _console.WriteLine(UserMessages.UnexpectedError);
                continue;
            }

            if (action == CommandAction.Quit)
                _running = false;
            else if (action == CommandAction.MoveAccepted && game.IsComplete())
                CompleteGameAndReplay();
        }
    }

    private bool TryStartNewGame()
    {
        ISudokuGame game;
        try
        {
            game = _gameFactory.Create();
            _game = game;
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            _exceptionLogger.Log(ex, "Starting a new game");
            _console.WriteLine(UserMessages.UnableToStartGame);
            return false;
        }

        _console.WriteLine(UserMessages.Welcome);
        _console.WriteLine();
        _console.WriteLine(UserMessages.PuzzleHeading);
        _commandHandler.PrintGrid(game);
        return true;
    }

    private void CompleteGameAndReplay()
    {
        _console.WriteLine();
        _console.WriteLine(UserMessages.GameCompleted);
        _console.WriteLine(UserMessages.ReplayPrompt);
        _console.ReadKey(true);
        _running = TryStartNewGame();
    }
}
