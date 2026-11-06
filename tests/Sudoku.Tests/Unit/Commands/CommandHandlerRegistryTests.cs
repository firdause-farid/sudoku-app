using Sudoku.Core.Application.Commands;
using Sudoku.Core.Domain.Game;

namespace Sudoku.Tests.Unit.Commands;

[Trait("Category", "Unit")]
public class CommandHandlerRegistryTests
{
    [Fact]
    public void TryFind_LiteralPattern_FindsRegisteredHandler()
    {
        var handler = new StubCommandHandler("new game now");
        var registry = new CommandHandlerRegistry([handler]);

        Assert.True(registry.TryFind("new game now", out ICommandHandler found));
        Assert.Same(handler, found);
    }

    [Fact]
    public void TryFind_PlaceAndClearPatterns_RoutesByLiteralBeforeWildcard()
    {
        var placeHandler = new StubCommandHandler("{cell} {number}");
        var clearHandler = new StubCommandHandler("{cell} clear");
        var registry = new CommandHandlerRegistry([placeHandler, clearHandler]);

        Assert.True(registry.TryFind("C5 clear", out ICommandHandler clearFound));
        Assert.Same(clearHandler, clearFound);

        Assert.True(registry.TryFind("A2 7", out ICommandHandler placeFound));
        Assert.Same(placeHandler, placeFound);
    }

    [Fact]
    public void TryFind_UnknownInput_ReturnsFalse()
    {
        var registry = new CommandHandlerRegistry([new StubCommandHandler("hint")]);

        Assert.False(registry.TryFind("new game", out _));
    }

    [Fact]
    public void Constructor_DuplicatePattern_Throws()
    {
        var handlers = new ICommandHandler[]
        {
            new StubCommandHandler("hint"),
            new StubCommandHandler("HINT")
        };

        Assert.Throws<ArgumentException>(() => new CommandHandlerRegistry(handlers));
    }

    private sealed class StubCommandHandler(string routePattern) : ICommandHandler
    {
        public string RoutePattern { get; } = routePattern;

        public bool TryHandle(string input, ISudokuGame game, out CommandAction action)
        {
            action = CommandAction.Continue;
            return false;
        }
    }
}
