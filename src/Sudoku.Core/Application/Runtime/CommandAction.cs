namespace Sudoku.Core.Application.Runtime;

/// <summary>
/// Tells the game loop what to do after a command has been handled.
/// </summary>
public enum CommandAction
{
    Continue,
    MoveAccepted,
    Quit
}
