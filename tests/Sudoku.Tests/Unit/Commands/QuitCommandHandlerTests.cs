using Sudoku.Core.Application.Commands;
using Sudoku.Core.Application.Commands.Handlers;
using Sudoku.Core.Presentation.Messages;
using Sudoku.Tests.Fakes;

namespace Sudoku.Tests.Unit.Commands;

[Trait("Category", "Unit")]
public class QuitCommandHandlerTests
{
    [Fact]
    public void Handle_Quit_ReturnsQuitAction()
    {
        var console = new FakeConsole();
        var handler = new QuitCommandHandler(console);

        CommandAction action = handler.Handle(CommandHandlerTestContext.Route(), CommandHandlerTestContext.CreateGame());

        Assert.Equal(CommandAction.Quit, action);
        Assert.Equal(UserMessages.ThanksForPlaying, Assert.Single(console.Outputs));
    }
}
