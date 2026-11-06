using Sudoku.Core.Application.Commands;
using Sudoku.Core.Domain.Game;

namespace Sudoku.Core.Application.Runtime;

/// <summary>
/// Handles player input for a running game and presents the result.
/// </summary>
public interface IConsoleCommandHandler
{
    CommandAction Handle(string input, ISudokuGame game);
    void PrintGrid(ISudokuGame game);
}
