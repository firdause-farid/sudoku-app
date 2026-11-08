using Sudoku.Core.Application.Ports;
using Sudoku.Core.Domain.Game;
using Sudoku.Core.Domain.Model;
using Sudoku.Core.Presentation.Messages;

namespace Sudoku.Core.Application.Commands.Handlers;

public sealed class ClearCommandHandler(IConsole console, IRenderer renderer) : ICommandHandler
{
    private const string Pattern = $"{{{CommandHandlerInput.CellParameter}}} clear";

    public string RoutePattern => Pattern;

    public CommandAction Handle(IReadOnlyDictionary<string, string> routeValues, ISudokuGame game)
    {
        SudokuDefinition definition = game.GetGridView().Definition;
        if (!CommandHandlerInput.TryGetPosition(routeValues[CommandHandlerInput.CellParameter], definition, out Position position, out string errorMessage))
        {
            console.WriteLine(UserMessages.InvalidCommand(errorMessage));
            return CommandAction.Continue;
        }

        MoveResult result = game.Clear(position);
        string message = result.Status == MoveStatus.PrefilledCell
            ? UserMessages.PrefilledCell(definition.FormatPosition(position))
            : UserMessages.MoveAccepted;
        CommandOutput.WriteMessageWithGrid(console, renderer, game, message);
        return CommandAction.Continue;
    }
}
