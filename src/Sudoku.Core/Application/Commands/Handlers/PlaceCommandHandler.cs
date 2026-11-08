using System.Globalization;
using Sudoku.Core.Application.Ports;
using Sudoku.Core.Domain.Game;
using Sudoku.Core.Domain.Model;
using Sudoku.Core.Presentation.Messages;

namespace Sudoku.Core.Application.Commands.Handlers;

public sealed class PlaceCommandHandler(IConsole console, IRenderer renderer) : ICommandHandler
{
    private const string Pattern = $"{{{CommandHandlerInput.CellParameter}}} {{{CommandHandlerInput.NumberParameter}}}";

    public string RoutePattern => Pattern;

    public CommandAction Handle(IReadOnlyDictionary<string, string> routeValues, ISudokuGame game)
    {
        SudokuDefinition definition = game.GetGridView().Definition;
        string numberText = routeValues[CommandHandlerInput.NumberParameter];
        if (!definition.TryParseValue(numberText, out int number))
        {
            // A whole number outside the grid's range is a move error; anything else is not a command.
            if (!int.TryParse(numberText, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                return CommandAction.Unrecognized;

            console.WriteLine(UserMessages.InvalidMoveNumberFor(definition.GridSize));
            return CommandAction.Continue;
        }

        if (!CommandHandlerInput.TryGetPosition(routeValues[CommandHandlerInput.CellParameter], definition, out Position position, out string errorMessage))
        {
            console.WriteLine(UserMessages.InvalidCommand(errorMessage));
            return CommandAction.Continue;
        }

        MoveResult result = game.Place(position, number);
        string message = result.Status switch
        {
            MoveStatus.Accepted => UserMessages.MoveAccepted,
            MoveStatus.InvalidNumber => UserMessages.InvalidMoveNumberFor(definition.GridSize),
            MoveStatus.PrefilledCell => UserMessages.PrefilledCell(definition.FormatPosition(position)),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "Unknown move status.")
        };
        CommandOutput.WriteMessageWithGrid(console, renderer, game, message);
        return result.Accepted ? CommandAction.MoveAccepted : CommandAction.Continue;
    }
}
