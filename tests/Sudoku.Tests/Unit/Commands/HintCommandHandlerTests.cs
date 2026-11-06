using Sudoku.Core.Application.Commands;
using Sudoku.Core.Application.Commands.Handlers;
using Sudoku.Tests.Fakes;

namespace Sudoku.Tests.Unit.Commands;

[Trait("Category", "Unit")]
public class HintCommandHandlerTests
{
    [Fact]
    public void TryHandle_Hint_ShowsHint()
    {
        var console = new FakeConsole();
        var handler = new HintCommandHandler(console);

        bool handled = handler.TryHandle("hint", CommandHandlerTestContext.CreateGame(), out CommandAction action);

        Assert.True(handled);
        Assert.Equal(CommandAction.Continue, action);
        Assert.StartsWith("Hint:", console.Outputs.Single());
    }

    [Fact]
    public void TryHandle_OtherInput_DeclinesIt()
    {
        var console = new FakeConsole();
        var handler = new HintCommandHandler(console);

        bool handled = handler.TryHandle("hint now", CommandHandlerTestContext.CreateGame(), out CommandAction action);

        Assert.False(handled);
        Assert.Equal(CommandAction.Continue, action);
        Assert.Empty(console.Outputs);
    }
}
