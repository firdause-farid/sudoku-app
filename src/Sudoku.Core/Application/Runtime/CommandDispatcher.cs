using Sudoku.Core.Application.Commands;
using Sudoku.Core.Application.Ports;
using Sudoku.Core.Domain.Game;
using Sudoku.Core.Presentation.Messages;

namespace Sudoku.Core.Application.Runtime;

/// <summary>Routes each line of input to its registered command handler. Input that matches no command gets the standard "unrecognized command" message.</summary>
public sealed class CommandDispatcher : ICommandDispatcher
{
    private readonly IConsole _console;
    private readonly IRenderer _renderer;
    private readonly CommandHandlerRegistry _handlerRegistry;

    public CommandDispatcher(
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
