using Sudoku.Core.Application.Commands;
using Sudoku.Core.Application.Ports;
using Sudoku.Core.Domain.Game;
using Sudoku.Core.Presentation.Messages;

namespace Sudoku.Core.Application.Runtime;

/// <summary>Parses input and dispatches each command to its registered handler.</summary>
public sealed class ConsoleCommandHandler : IConsoleCommandHandler
{
    private readonly IConsole _console;
    private readonly IRenderer _renderer;
    private readonly CommandHandlerRegistry _handlerRegistry;

    public ConsoleCommandHandler(
        IConsole console,
        IRenderer renderer,
        IEnumerable<ICommandHandler> handlers)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _handlerRegistry = new CommandHandlerRegistry(handlers);
    }

    public CommandAction Handle(string input, ISudokuGame game)
    {
        if (_handlerRegistry.TryFind(input, out ICommandHandler handler) &&
            handler.TryHandle(input, game, out CommandAction action))
        {
            return action;
        }

        _console.WriteLine(UserMessages.InvalidCommand(UserMessages.UnrecognizedCommand));
        _console.WriteLine();
        _console.WriteLine(UserMessages.CurrentGridHeader);
        PrintGrid(game);
        return CommandAction.Continue;
    }

    public void PrintGrid(ISudokuGame game) => _console.WriteLine(_renderer.Render(game.GetGridView()));
}
