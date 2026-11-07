using Sudoku.Core.Application.Ports;
using Sudoku.Core.Domain.Game;
using Sudoku.Core.Presentation.Messages;

namespace Sudoku.Core.Application.Commands;

/// <summary>Shared console output used by several command handlers.</summary>
internal static class CommandOutput
{
    /// <summary>Writes a message, a blank line, then the current grid.</summary>
    public static void WriteMessageWithGrid(IConsole console, IRenderer renderer, ISudokuGame game, string message)
    {
        console.WriteLine(message);
        console.WriteLine();
        console.WriteLine(UserMessages.CurrentGridHeader);
        console.WriteLine(renderer.Render(game.GetGridView()));
    }
}
