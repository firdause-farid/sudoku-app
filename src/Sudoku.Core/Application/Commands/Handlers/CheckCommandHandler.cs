using Sudoku.Core.Application.Ports;
using Sudoku.Core.Application.Runtime;
using Sudoku.Core.Domain.Game;
using Sudoku.Core.Domain.Model;
using Sudoku.Core.Domain.Validation;
using Sudoku.Core.Presentation.Messages;

namespace Sudoku.Core.Application.Commands.Handlers;

public sealed class CheckCommandHandler(IConsole console) : ICommandHandler
{
    public string RoutePattern => "check";

    public bool TryHandle(string input, ISudokuGame game, out CommandAction action)
    {
        action = CommandAction.Continue;
        if (!input.Trim().Equals("check", StringComparison.OrdinalIgnoreCase))
            return false;

        ValidationResult result = game.Check();
        if (result.IsValid)
            console.WriteLine(UserMessages.NoRuleViolations);
        else
        {
            ValidationIssue issue = result.Issue
                ?? throw new InvalidOperationException("An invalid validation result must include an issue.");
            console.WriteLine(issue.Type switch
            {
                ValidationIssueType.DuplicateRow when issue.UnitIndex is int row =>
                    UserMessages.DuplicateRow(issue.Value, new Position(row, 0).RowLetter),
                ValidationIssueType.DuplicateColumn when issue.UnitIndex is int column =>
                    UserMessages.DuplicateColumn(issue.Value, new Position(0, column).ColumnNumber),
                ValidationIssueType.DuplicateSubgrid => UserMessages.DuplicateSubgrid(issue.Value),
                _ => throw new ArgumentOutOfRangeException(nameof(issue), issue.Type, "Unknown validation issue.")
            });
        }
        return true;
    }
}
