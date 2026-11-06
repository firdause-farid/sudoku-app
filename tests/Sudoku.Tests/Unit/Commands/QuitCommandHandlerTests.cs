using Sudoku.Core.Application.Commands;
using Sudoku.Core.Application.Commands.Handlers;
using Sudoku.Core.Presentation.Messages;
using Sudoku.Tests.Fakes;

namespace Sudoku.Tests.Unit.Commands;

[Trait("Category", "Unit")]
public class QuitCommandHandlerTests
{
    [Fact]
    public void TryHandle_Quit_ReturnsQuitAction()
    {
        var console = new FakeConsole();
        var handler = new QuitCommandHandler(console);

        bool handled = handler.TryHandle("quit", CommandHandlerTestContext.CreateGame(), out CommandAction action);

        Assert.True(handled);
        Assert.Equal(CommandAction.Quit, action);
        Assert.Equal(UserMessages.ThanksForPlaying, Assert.Single(console.Outputs));
    }

    [Fact]
    public void TryHandle_QuitWithExtraText_DeclinesIt()
    {
        var console = new FakeConsole();
        var handler = new QuitCommandHandler(console);

        bool handled = handler.TryHandle("quit now", CommandHandlerTestContext.CreateGame(), out CommandAction action);

        Assert.False(handled);
        Assert.Equal(CommandAction.Continue, action);
        Assert.Empty(console.Outputs);
    }
}
