using Sudoku.Core.Application.Ports;
using Sudoku.Core.Domain.Game;
using Sudoku.Core.Presentation.Messages;

namespace Sudoku.Core.Application.Commands.Handlers;

public sealed class QuitCommandHandler(IConsole console) : ICommandHandler
{
    public string RoutePattern => "quit";

    public bool TryHandle(string input, ISudokuGame game, out CommandAction action)
    {
        if (input.Trim().Equals(RoutePattern, StringComparison.OrdinalIgnoreCase))
        {
            console.WriteLine(UserMessages.ThanksForPlaying);
            action = CommandAction.Quit;
            return true;
        }

        action = CommandAction.Continue;
        return false;
    }
}
