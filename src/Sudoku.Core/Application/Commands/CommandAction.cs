namespace Sudoku.Core.Application.Commands;

/// <summary>
/// Tells the game loop what to do after a command has been handled.
/// </summary>
public enum CommandAction
{
    Continue,
    MoveAccepted,
    Quit,

    /// <summary>The input matched a route but is not a valid command (for example "A1 abc").</summary>
    Unrecognized
}
