using Sudoku.Core.Application.Commands;
using Sudoku.Core.Domain.Game;

namespace Sudoku.Core.Application.Runtime;

/// <summary>
/// Sends a line of player input to the matching command for a running game and shows the outcome.
/// </summary>
public interface ICommandDispatcher
{
    CommandAction Handle(string input, ISudokuGame game);
    void PrintGrid(ISudokuGame game);
}
