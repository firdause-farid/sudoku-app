using Sudoku.Core.Application.Commands;
using Sudoku.Core.Application.Commands.Handlers;
using Sudoku.Core.Domain.Model;
using Sudoku.Tests.Fakes;

namespace Sudoku.Tests.Unit.Commands;

[Trait("Category", "Unit")]
public class CheckCommandHandlerTests
{
    [Fact]
    public void TryHandle_Check_ReportsRuleViolation()
    {
        var console = new FakeConsole();
        var game = CommandHandlerTestContext.CreateGame();
        game.Place(new Position(0, 0), 5);
        game.Place(new Position(0, 1), 5);
        var handler = new CheckCommandHandler(console);

        bool handled = handler.TryHandle("check", game, out CommandAction action);

        Assert.True(handled);
        Assert.Equal(CommandAction.Continue, action);
        Assert.Contains("Number 5 already exists in Row A.", console.Outputs);
    }

    [Fact]
    public void TryHandle_DifferentInput_DeclinesIt()
    {
        var console = new FakeConsole();
        var handler = new CheckCommandHandler(console);

        bool handled = handler.TryHandle("check now", CommandHandlerTestContext.CreateGame(), out CommandAction action);

        Assert.False(handled);
        Assert.Equal(CommandAction.Continue, action);
        Assert.Empty(console.Outputs);
    }
}
