namespace Sudoku.Core.Domain.Game;

/// <summary>
/// Generates and creates a fresh Sudoku game.
/// </summary>
public interface ISudokuGameFactory
{
    ISudokuGame Create();
}
