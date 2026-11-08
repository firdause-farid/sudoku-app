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
    public void Handle_ValidCellAndNumber_PlacesValue()
    {
        var console = new FakeConsole();
        var game = CommandHandlerTestContext.CreateGame();
        var handler = new PlaceCommandHandler(console, CommandHandlerTestContext.CreateRenderer());

        CommandAction action = handler.Handle(CommandHandlerTestContext.Route(("cell", "A1"), ("number", "5")), game);

        Assert.Equal(CommandAction.MoveAccepted, action);
        Assert.Equal(5, game.GetGridView().GetValue(new Position(0, 0)));
        Assert.Contains("Move accepted.", console.Outputs);
        Assert.Contains("rendered grid", console.Outputs);
    }

    [Fact]
    public void Handle_InvalidCellWithValidNumber_ReportsCellError()
    {
        var console = new FakeConsole();
        var game = CommandHandlerTestContext.CreateGame();
        var handler = new PlaceCommandHandler(console, CommandHandlerTestContext.CreateRenderer());

        CommandAction action = handler.Handle(CommandHandlerTestContext.Route(("cell", "Z1"), ("number", "5")), game);

        Assert.Equal(CommandAction.Continue, action);
        Assert.Contains(UserMessages.InvalidCommand(UserMessages.RowRangeFor("A", "I")), console.Outputs);
    }

    [Fact]
    public void Handle_NumberOutOfRange_ReportsNumberError()
    {
        var console = new FakeConsole();
        var game = CommandHandlerTestContext.CreateGame();
        var handler = new PlaceCommandHandler(console, CommandHandlerTestContext.CreateRenderer());

        CommandAction action = handler.Handle(CommandHandlerTestContext.Route(("cell", "A1"), ("number", "10")), game);

        Assert.Equal(CommandAction.Continue, action);
        Assert.Contains(UserMessages.InvalidMoveNumberFor(9), console.Outputs);
        Assert.Null(game.GetGridView().GetValue(new Position(0, 0)));
    }

    [Fact]
    public void Handle_NumberIsNotANumber_IsUnrecognized()
    {
        var console = new FakeConsole();
        var handler = new PlaceCommandHandler(console, CommandHandlerTestContext.CreateRenderer());

        CommandAction action = handler.Handle(
            CommandHandlerTestContext.Route(("cell", "A1"), ("number", "abc")), CommandHandlerTestContext.CreateGame());

        Assert.Equal(CommandAction.Unrecognized, action);
        Assert.Empty(console.Outputs);
    }
}
