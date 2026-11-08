namespace Sudoku.Core.Domain.Game.Results;

/// <summary>
/// The outcome of a place or clear request.
/// </summary>
public sealed record MoveResult(MoveStatus Status)
{
    public bool Accepted => Status == MoveStatus.Accepted;
}
