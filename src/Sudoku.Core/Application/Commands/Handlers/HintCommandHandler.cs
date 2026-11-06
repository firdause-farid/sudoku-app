using Sudoku.Core.Application.Ports;
using Sudoku.Core.Application.Runtime;
using Sudoku.Core.Domain.Game;
using Sudoku.Core.Presentation.Messages;

namespace Sudoku.Core.Application.Commands.Handlers;

public sealed class HintCommandHandler(IConsole console) : ICommandHandler
{
    public string RoutePattern => "hint";

    public bool TryHandle(string input, ISudokuGame game, out CommandAction action)
    {
        action = CommandAction.Continue;
        if (!input.Trim().Equals(RoutePattern, StringComparison.OrdinalIgnoreCase))
            return false;

        HintResult? hint = game.Hint();
        console.WriteLine(hint is null
            ? UserMessages.NoEmptyCellsToHint
            : UserMessages.Hint(hint.Position.ToString(), hint.Value));
        return true;
    }
}
