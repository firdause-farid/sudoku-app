using Sudoku.Core.Application.Commands;
using Sudoku.Core.Application.Commands.Handlers;
using Sudoku.Core.Domain.Model;
using Sudoku.Core.Presentation.Messages;
using Sudoku.Tests.Fakes;

namespace Sudoku.Tests.Unit.Commands;

[Trait("Category", "Unit")]
public class ClearCommandHandlerTests
{
    [Fact]
    public void Handle_ValidCellAndClear_ClearsValue()
    {
        var console = new FakeConsole();
        var game = CommandHandlerTestContext.CreateGame();
        var position = new Position(0, 0);
        game.Place(position, 5);
        var handler = new ClearCommandHandler(console, CommandHandlerTestContext.CreateRenderer());

        CommandAction action = handler.Handle(CommandHandlerTestContext.Route(("cell", "A1")), game);

        Assert.Equal(CommandAction.Continue, action);
        Assert.Null(game.GetGridView().GetValue(position));
        Assert.Contains("Move accepted.", console.Outputs);
        Assert.Contains("rendered grid", console.Outputs);
    }

    [Fact]
    public void Handle_InvalidCellWithValidClear_ReportsCellError()
    {
        var console = new FakeConsole();
        var game = CommandHandlerTestContext.CreateGame();
        var handler = new ClearCommandHandler(console, CommandHandlerTestContext.CreateRenderer());

        CommandAction action = handler.Handle(CommandHandlerTestContext.Route(("cell", "Z1")), game);

        Assert.Equal(CommandAction.Continue, action);
        Assert.Contains(UserMessages.InvalidCommand(UserMessages.RowRange), console.Outputs);
    }
}
