using Sudoku.Core.Application.Ports;
using Sudoku.Core.Application.Runtime;
using Sudoku.Core.Domain.Game;
using Sudoku.Core.Domain.Model;
using Sudoku.Core.Presentation.Messages;
using Sudoku.Core.Presentation.Rendering;

namespace Sudoku.Core.Application.Commands.Handlers;

public sealed class PlaceCommandHandler(IConsole console, IRenderer renderer) : ICommandHandler
{
    public string RoutePattern => "{cell} {number}";

    public bool TryHandle(string input, ISudokuGame game, out CommandAction action)
    {
        action = CommandAction.Continue;
        if (!CommandHandlerInput.TryGetParts(input, out string[] parts) ||
            !int.TryParse(parts[1], out int number))
            return false;

        if (!CommandHandlerInput.TryGetPosition(parts[0], out Position position, out string errorMessage))
        {
            console.WriteLine(UserMessages.InvalidCommand(errorMessage));
            return true;
        }

        MoveResult result = game.Place(position, number);
        string message = result.Status switch
        {
            MoveStatus.Accepted => UserMessages.MoveAccepted,
            MoveStatus.InvalidNumber => UserMessages.InvalidMoveNumber,
            MoveStatus.PrefilledCell => UserMessages.PrefilledCell(position.ToString()),
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
