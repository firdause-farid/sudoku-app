using Sudoku.Core.Application.Commands;
using Sudoku.Core.Application.Commands.Handlers;
using Sudoku.Core.Domain.Model;
using Sudoku.Tests.Fakes;

namespace Sudoku.Tests.Unit.Commands;

[Trait("Category", "Unit")]
public class CheckCommandHandlerTests
{
    [Fact]
    public void Handle_Check_ReportsRuleViolation()
    {
        var console = new FakeConsole();
        var game = CommandHandlerTestContext.CreateGame();
        game.Place(new Position(0, 0), 5);
        game.Place(new Position(0, 1), 5);
        var handler = new CheckCommandHandler(console);

        CommandAction action = handler.Handle(CommandHandlerTestContext.Route(), game);

        Assert.Equal(CommandAction.Continue, action);
        Assert.Contains("Number 5 already exists in Row A.", console.Outputs);
    }
}
