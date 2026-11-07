using Sudoku.Core.Application.Commands;
using Sudoku.Core.Application.Commands.Handlers;
using Sudoku.Tests.Fakes;

namespace Sudoku.Tests.Unit.Commands;

[Trait("Category", "Unit")]
public class HintCommandHandlerTests
{
    [Fact]
    public void Handle_Hint_ShowsHint()
    {
        var console = new FakeConsole();
        var handler = new HintCommandHandler(console);

        CommandAction action = handler.Handle(CommandHandlerTestContext.Route(), CommandHandlerTestContext.CreateGame());

        Assert.Equal(CommandAction.Continue, action);
        Assert.StartsWith("Hint:", console.Outputs.Single());
    }
}
