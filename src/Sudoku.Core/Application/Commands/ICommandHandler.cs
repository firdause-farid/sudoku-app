using Sudoku.Core.Domain.Game;

namespace Sudoku.Core.Application.Commands;

/// <summary>
/// Executes one command. The <see cref="CommandHandlerRegistry"/> has already matched the input
/// to <see cref="RoutePattern"/>, so a handler never re-checks the command word.
/// </summary>
public interface ICommandHandler
{
    /// <summary>Token pattern used by the router, e.g. "hint" or "{cell} {number}".</summary>
    string RoutePattern { get; }

    /// <summary>Runs the command. <paramref name="routeValues"/> holds the text matched by each {parameter}.</summary>
    CommandAction Handle(IReadOnlyDictionary<string, string> routeValues, ISudokuGame game);
}
