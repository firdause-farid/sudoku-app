using Sudoku.Core.Application.Ports;
using Sudoku.Core.Domain.Game;
using Sudoku.Core.Domain.Model;
using Sudoku.Core.Presentation.Messages;

namespace Sudoku.Core.Application.Commands.Handlers;

public sealed class ClearCommandHandler(IConsole console, IRenderer renderer) : ICommandHandler
{
    private const string ClearRoutePattern = "{cell} clear";
    private static readonly string ClearKeyword = ClearRoutePattern.Split(' ')[1];

    public string RoutePattern => ClearRoutePattern;

    public bool TryHandle(string input, ISudokuGame game, out CommandAction action)
    {
        action = CommandAction.Continue;
        if (!CommandHandlerInput.TryGetParts(input, expectedPartCount: 2, out string[] parts) ||
            !parts[1].Equals(ClearKeyword, StringComparison.OrdinalIgnoreCase))
            return false;

        SudokuDefinition definition = game.GetGridView().Definition;
        if (!CommandHandlerInput.TryGetPosition(parts[0], definition, out Position position, out string errorMessage))
        {
            console.WriteLine(UserMessages.InvalidCommand(errorMessage));
            return true;
        }

        MoveResult result = game.Clear(position);
        console.WriteLine(result.Status == MoveStatus.PrefilledCell
            ? UserMessages.PrefilledCell(definition.FormatPosition(position))
            : UserMessages.MoveAccepted);
        console.WriteLine();
        console.WriteLine(UserMessages.CurrentGridHeader);
        console.WriteLine(renderer.Render(game.GetGridView()));
        return true;
    }
}
