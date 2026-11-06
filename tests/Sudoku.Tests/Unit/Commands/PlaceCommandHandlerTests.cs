using Sudoku.Core.Application.Commands;
using Sudoku.Core.Application.Commands.Handlers;
using Sudoku.Core.Domain.Model;
using Sudoku.Core.Presentation.Messages;
using Sudoku.Tests.Fakes;

namespace Sudoku.Tests.Unit.Commands;

[Trait("Category", "Unit")]
public class PlaceCommandHandlerTests
{
    [Fact]
    public void TryHandle_ValidCellAndNumber_PlacesValue()
    {
        var console = new FakeConsole();
        var game = CommandHandlerTestContext.CreateGame();
        var handler = new PlaceCommandHandler(console, CommandHandlerTestContext.CreateRenderer());

        bool handled = handler.TryHandle("A1 5", game, out CommandAction action);

        Assert.True(handled);
        Assert.Equal(CommandAction.MoveAccepted, action);
        Assert.Equal(5, game.GetGridView().GetValue(new Position(0, 0)));
        Assert.Contains("Move accepted.", console.Outputs);
        Assert.Contains("rendered grid", console.Outputs);
    }

    [Fact]
    public void TryHandle_InvalidCellWithValidNumber_ReportsCellError()
    {
        var console = new FakeConsole();
        var game = CommandHandlerTestContext.CreateGame();
        var handler = new PlaceCommandHandler(console, CommandHandlerTestContext.CreateRenderer());

        bool handled = handler.TryHandle("Z1 5", game, out CommandAction action);

        Assert.True(handled);
        Assert.Equal(CommandAction.Continue, action);
        Assert.Contains(UserMessages.InvalidCommand(UserMessages.RowRange), console.Outputs);
    }
}
