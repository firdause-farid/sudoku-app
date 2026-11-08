using Sudoku.Core.Application.Ports;
using Sudoku.Core.Domain.Game;
using Sudoku.Core.Domain.Game.Results;
using Sudoku.Core.Domain.Model;
using Sudoku.Core.Presentation.Messages;

namespace Sudoku.Core.Application.Commands.Handlers;

public sealed class HintCommandHandler(IConsole console) : ICommandHandler
{
    public string RoutePattern => "hint";

    public CommandAction Handle(IReadOnlyDictionary<string, string> routeValues, ISudokuGame game)
    {
        HintResult? hint = game.Hint();
        SudokuDefinition definition = game.GetGridView().Definition;
        console.WriteLine(hint is null
            ? UserMessages.NoEmptyCellsToHint
            : UserMessages.Hint(definition.FormatPosition(hint.Position), definition.FormatValue(hint.Value)));
        return CommandAction.Continue;
    }
}
