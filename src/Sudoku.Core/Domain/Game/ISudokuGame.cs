using Sudoku.Core.Domain.Model;
using Sudoku.Core.Domain.Validation;

namespace Sudoku.Core.Domain.Game;

/// <summary>
/// Exposes the operations required to play and present a Sudoku game.
/// </summary>
public interface ISudokuGame
{
    IGridView GetGridView();
    MoveResult Place(Position position, int number);
    MoveResult Clear(Position position);
    HintResult? Hint();
    ValidationResult Check();
    bool IsComplete();
}
