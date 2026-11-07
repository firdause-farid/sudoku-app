using Sudoku.Core.Application.Ports;
using Sudoku.Core.Domain.Game;
using Sudoku.Core.Presentation.Messages;

namespace Sudoku.Core.Application.Commands.Handlers;

public sealed class QuitCommandHandler(IConsole console) : ICommandHandler
{
    public string RoutePattern => "quit";

    public CommandAction Handle(IReadOnlyDictionary<string, string> routeValues, ISudokuGame game)
    {
        console.WriteLine(UserMessages.ThanksForPlaying);
        return CommandAction.Quit;
    }
}
