using Sudoku.Core.Application.Commands;
using Sudoku.Core.Application.Ports;
using Sudoku.Core.Domain.Game;
using Sudoku.Core.Domain.Model;
using Sudoku.Core.Domain.Validation;
using Sudoku.Core.Presentation.Messages;
using Sudoku.Core.Presentation.Rendering;

namespace Sudoku.Core.Application.Runtime;

/// <summary>
/// Executes one parsed player command and presents its feedback.
/// </summary>
public sealed class ConsoleCommandHandler : IConsoleCommandHandler
{
    private readonly IConsole _console;
    private readonly IRenderer _renderer;

    public ConsoleCommandHandler(IConsole console, IRenderer renderer)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
    }

    public CommandAction Handle(string input, ISudokuGame game)
    {
        if (!CommandParser.TryParse(input, out Command command, out CommandParseError error))
        {
            _console.WriteLine(UserMessages.InvalidCommand(FormatCommandParseError(error)));
            _console.WriteLine();
            _console.WriteLine(UserMessages.CurrentGridHeader);
            PrintGrid(game);
            return CommandAction.Continue;
        }

        switch (command)
        {
            case PlaceCommand place:
                return HandlePlace(game, place.Position, place.Number);
            case ClearCommand clear:
                HandleClear(game, clear.Position);
                return CommandAction.Continue;
            case HintCommand:
                HandleHint(game);
                return CommandAction.Continue;
            case CheckCommand:
                HandleCheck(game);
                return CommandAction.Continue;
            case QuitCommand:
                _console.WriteLine(UserMessages.ThanksForPlaying);
                return CommandAction.Quit;
            default:
                throw new ArgumentOutOfRangeException(nameof(command), command, "Unknown command.");
        }
    }

    public void PrintGrid(ISudokuGame game)
    {
        _console.WriteLine(_renderer.Render(game.GetGridView()));
    }

    private CommandAction HandlePlace(ISudokuGame game, Position position, int number)
    {
        MoveResult result = game.Place(position, number);
        DisplayMoveResultAndGrid(game, result, position);
        return result.Accepted ? CommandAction.MoveAccepted : CommandAction.Continue;
    }

    private void HandleClear(ISudokuGame game, Position position)
    {
        MoveResult result = game.Clear(position);
        DisplayMoveResultAndGrid(game, result, position);
    }

    private void DisplayMoveResultAndGrid(ISudokuGame game, MoveResult result, Position position)
    {
        _console.WriteLine(FormatMoveResult(result, position));
        _console.WriteLine();
        _console.WriteLine(UserMessages.CurrentGridHeader);
        PrintGrid(game);
    }

    private void HandleHint(ISudokuGame game)
    {
        HintResult? hint = game.Hint();
        if (hint is null)
        {
            _console.WriteLine(UserMessages.NoEmptyCellsToHint);
            return;
        }

        _console.WriteLine(UserMessages.Hint(hint.Position.ToString(), hint.Value));
    }

    private void HandleCheck(ISudokuGame game)
    {
        ValidationResult result = game.Check();
        if (result.IsValid)
            _console.WriteLine(UserMessages.NoRuleViolations);
        else
            _console.WriteLine(FormatValidationIssue(result.Issue
                ?? throw new InvalidOperationException("An invalid validation result must include an issue.")));
    }

    private static string FormatMoveResult(MoveResult result, Position position) => result.Status switch
    {
        MoveStatus.Accepted => UserMessages.MoveAccepted,
        MoveStatus.InvalidNumber => UserMessages.InvalidMoveNumber,
        MoveStatus.PrefilledCell => UserMessages.PrefilledCell(position.ToString()),
        _ => throw new ArgumentOutOfRangeException(nameof(result), result.Status, "Unknown move status.")
    };

    private static string FormatValidationIssue(ValidationIssue issue) => issue.Type switch
    {
        ValidationIssueType.DuplicateRow when issue.UnitIndex is int row =>
            UserMessages.DuplicateRow(issue.Value, new Position(row, 0).RowLetter),
        ValidationIssueType.DuplicateColumn when issue.UnitIndex is int column =>
            UserMessages.DuplicateColumn(issue.Value, new Position(0, column).ColumnNumber),
        ValidationIssueType.DuplicateSubgrid => UserMessages.DuplicateSubgrid(issue.Value),
        _ => throw new ArgumentOutOfRangeException(nameof(issue), issue.Type, "Unknown validation issue.")
    };

    private static string FormatCommandParseError(CommandParseError error) => error switch
    {
        CommandParseError.CommandRequired => UserMessages.CommandRequired,
        CommandParseError.UnrecognizedCommand => UserMessages.UnrecognizedCommand,
        CommandParseError.PositionRequired => UserMessages.PositionRequired,
        CommandParseError.InvalidPositionFormat => UserMessages.PositionFormat,
        CommandParseError.RowOutOfRange => UserMessages.RowRange,
        CommandParseError.ColumnOutOfRange => UserMessages.ColumnRange,
        _ => UserMessages.UnrecognizedCommand
    };
}
