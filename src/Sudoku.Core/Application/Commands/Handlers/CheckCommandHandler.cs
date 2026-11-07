using Sudoku.Core.Application.Ports;
using Sudoku.Core.Domain.Game;
using Sudoku.Core.Domain.Model;
using Sudoku.Core.Domain.Validation;
using Sudoku.Core.Presentation.Messages;

namespace Sudoku.Core.Application.Commands.Handlers;

public sealed class CheckCommandHandler(IConsole console) : ICommandHandler
{
    public string RoutePattern => "check";

    public CommandAction Handle(IReadOnlyDictionary<string, string> routeValues, ISudokuGame game)
    {
        ValidationResult result = game.Check();
        if (result.IsValid)
            console.WriteLine(UserMessages.NoRuleViolations);
        else
        {
            SudokuDefinition definition = game.GetGridView().Definition;
            ValidationIssue issue = result.Issue
                ?? throw new InvalidOperationException("An invalid validation result must include an issue.");
            console.WriteLine(issue.Type switch
            {
                ValidationIssueType.DuplicateRow when issue.UnitIndex is int row =>
                    UserMessages.DuplicateRow(definition.FormatValue(issue.Value), definition.RowLabels[row]),
                ValidationIssueType.DuplicateColumn when issue.UnitIndex is int column =>
                    UserMessages.DuplicateColumn(definition.FormatValue(issue.Value), column + 1),
                ValidationIssueType.DuplicateSubgrid => UserMessages.DuplicateSubgrid(
                    definition.FormatValue(issue.Value), definition.SubgridRows, definition.SubgridColumns),
                _ => throw new ArgumentOutOfRangeException(nameof(issue), issue.Type, "Unknown validation issue.")
            });
        }
        return CommandAction.Continue;
    }
}
