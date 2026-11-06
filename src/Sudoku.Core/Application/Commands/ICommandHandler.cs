using Sudoku.Core.Domain.Game;

namespace Sudoku.Core.Application.Commands;

/// <summary>Recognizes and handles one command syntax.</summary>
public interface ICommandHandler
{
    /// <summary>Token pattern used by the router to select this handler.</summary>
    string RoutePattern { get; }
    bool TryHandle(string input, ISudokuGame game, out CommandAction action);
}
