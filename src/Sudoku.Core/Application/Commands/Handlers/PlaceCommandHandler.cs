using Sudoku.Core.Application.Ports;
using Sudoku.Core.Domain.Game;
using Sudoku.Core.Domain.Model;
using Sudoku.Core.Presentation.Messages;

namespace Sudoku.Core.Application.Commands.Handlers;

public sealed class PlaceCommandHandler(IConsole console, IRenderer renderer) : ICommandHandler
{
    public string RoutePattern => "{cell} {number}";

    public bool TryHandle(string input, ISudokuGame game, out CommandAction action)
    {
        action = CommandAction.Continue;
        if (!CommandHandlerInput.TryGetParts(input, expectedPartCount: 2, out string[] parts))
            return false;

        SudokuDefinition definition = game.GetGridView().Definition;
        if (!definition.TryParseValue(parts[1], out int number))
        {
            if (int.TryParse(parts[1], out _))
            {
                console.WriteLine(UserMessages.InvalidMoveNumberFor(definition.GridSize));
                return true;
            }
            return false;
        }

        if (!CommandHandlerInput.TryGetPosition(parts[0], definition, out Position position, out string errorMessage))
        {
            console.WriteLine(UserMessages.InvalidCommand(errorMessage));
            return true;
        }

        MoveResult result = game.Place(position, number);
        string message = result.Status switch
        {
            MoveStatus.Accepted => UserMessages.MoveAccepted,
            MoveStatus.InvalidNumber => UserMessages.InvalidMoveNumberFor(definition.GridSize),
            MoveStatus.PrefilledCell => UserMessages.PrefilledCell(definition.FormatPosition(position)),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "Unknown move status.")
        };
        console.WriteLine(message);
        console.WriteLine();
        console.WriteLine(UserMessages.CurrentGridHeader);
        console.WriteLine(renderer.Render(game.GetGridView()));
        action = result.Accepted ? CommandAction.MoveAccepted : CommandAction.Continue;
        return true;
    }
}
