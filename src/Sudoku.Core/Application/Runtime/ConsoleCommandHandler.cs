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
        if (_handlerRegistry.TryFind(input, out ICommandHandler handler, out var routeValues))
        {
            CommandAction action = handler.Handle(routeValues, game);
            if (action != CommandAction.Unrecognized)
                return action;
        }

        CommandOutput.WriteMessageWithGrid(
            _console, _renderer, game, UserMessages.InvalidCommand(UserMessages.UnrecognizedCommand));
        return CommandAction.Continue;
    }

    public void PrintGrid(ISudokuGame game) => _console.WriteLine(_renderer.Render(game.GetGridView()));
}
