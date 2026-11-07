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

        Assert.True(registry.TryFind("new game now", out ICommandHandler found, out _));
        Assert.Same(handler, found);
    }

    [Fact]
    public void TryFind_PlaceAndClearPatterns_RoutesByLiteralBeforeWildcard()
    {
        var placeHandler = new StubCommandHandler("{cell} {number}");
        var clearHandler = new StubCommandHandler("{cell} clear");
        var registry = new CommandHandlerRegistry([placeHandler, clearHandler]);

        Assert.True(registry.TryFind("C5 clear", out ICommandHandler clearFound, out var clearValues));
        Assert.Same(clearHandler, clearFound);
        Assert.Equal("C5", clearValues["cell"]);

        Assert.True(registry.TryFind("A2 7", out ICommandHandler placeFound, out var placeValues));
        Assert.Same(placeHandler, placeFound);
        Assert.Equal("A2", placeValues["cell"]);
        Assert.Equal("7", placeValues["number"]);
    }

    [Fact]
    public void TryFind_UnknownInput_ReturnsFalse()
    {
        var registry = new CommandHandlerRegistry([new StubCommandHandler("hint")]);

        Assert.False(registry.TryFind("new game", out _, out _));
    }

    [Fact]
    public void TryFind_ExtraWords_ReturnsFalse()
    {
        var registry = new CommandHandlerRegistry(
            [new StubCommandHandler("hint"), new StubCommandHandler("{cell} {number}"), new StubCommandHandler("{cell} clear")]);

        Assert.False(registry.TryFind("hint now", out _, out _));
        Assert.False(registry.TryFind("A1 clear now", out _, out _));
        Assert.False(registry.TryFind("A1", out _, out _));
    }

    [Fact]
    public void TryFind_IsCaseInsensitive()
    {
        var handler = new StubCommandHandler("{cell} clear");
        var registry = new CommandHandlerRegistry([handler]);

        Assert.True(registry.TryFind("a1 CLEAR", out ICommandHandler found, out _));
        Assert.Same(handler, found);
    }

    [Fact]
    public void TryFind_DifferentParameterNamesInSamePosition_EachRouteUsesItsOwnNames()
    {
        var clearHandler = new StubCommandHandler("{cell} clear");
        var renameHandler = new StubCommandHandler("{player} rename {name}");
        var registry = new CommandHandlerRegistry([clearHandler, renameHandler]);

        Assert.True(registry.TryFind("B2 clear", out ICommandHandler clearFound, out var clearValues));
        Assert.Same(clearHandler, clearFound);
        Assert.Equal("B2", clearValues["cell"]);
        Assert.False(clearValues.ContainsKey("player"));

        Assert.True(registry.TryFind("sam rename Samuel", out ICommandHandler renameFound, out var renameValues));
        Assert.Same(renameHandler, renameFound);
        Assert.Equal("sam", renameValues["player"]);
        Assert.Equal("Samuel", renameValues["name"]);
    }

    [Fact]
    public void Constructor_SameShapeWithDifferentNames_Throws()
    {
        var handlers = new ICommandHandler[]
        {
            new StubCommandHandler("{cell} clear"),
            new StubCommandHandler("{player} clear")
        };

        Assert.Throws<ArgumentException>(() => new CommandHandlerRegistry(handlers));
    }

    [Fact]
    public void Constructor_RepeatedParameterName_Throws()
    {
        Assert.Throws<ArgumentException>(() => new CommandHandlerRegistry([new StubCommandHandler("{cell} {cell}")]));
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

        public CommandAction Handle(IReadOnlyDictionary<string, string> routeValues, ISudokuGame game) =>
            CommandAction.Continue;
    }
}
