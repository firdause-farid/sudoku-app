using Sudoku.Core.Application.Ports;
using Sudoku.Core.Application.Runtime;
using Sudoku.Core.Domain.Game;
using Sudoku.Core.Domain.Model;
using Sudoku.Core.Presentation.Messages;
using Sudoku.Core.Presentation.Rendering;

namespace Sudoku.Core.Application.Commands.Handlers;

public sealed class ClearCommandHandler(IConsole console, IRenderer renderer) : ICommandHandler
{
    public string RoutePattern => "{cell} clear";

    public bool TryHandle(string input, ISudokuGame game, out CommandAction action)
    {
        action = CommandAction.Continue;
        if (!CommandHandlerInput.TryGetParts(input, out string[] parts) ||
            !parts[1].Equals("clear", StringComparison.OrdinalIgnoreCase))
            return false;

        if (!CommandHandlerInput.TryGetPosition(parts[0], out Position position, out string errorMessage))
        {
            console.WriteLine(UserMessages.InvalidCommand(errorMessage));
            return true;
        }

        MoveResult result = game.Clear(position);
        console.WriteLine(result.Status == MoveStatus.PrefilledCell
            ? UserMessages.PrefilledCell(position.ToString())
            : UserMessages.MoveAccepted);
        console.WriteLine();
        console.WriteLine(UserMessages.CurrentGridHeader);
        console.WriteLine(renderer.Render(game.GetGridView()));
        return true;
    }
}
