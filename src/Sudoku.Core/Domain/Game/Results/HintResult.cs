using Sudoku.Core.Domain.Model;

namespace Sudoku.Core.Domain.Game.Results;

/// <summary>
/// A correct value for one empty cell.
/// </summary>
public sealed record HintResult(Position Position, int Value);
