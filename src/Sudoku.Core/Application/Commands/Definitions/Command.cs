namespace Sudoku.Core.Application.Commands;

/// <summary>
/// A validated player command. Each kind of command is its own type, so a command can only
/// exist with exactly the data it needs (for example, a place command always has a position and a number).
/// </summary>
public abstract record Command
{
    // Keeps the set of commands closed to this assembly.
    private protected Command()
    {
    }
}
