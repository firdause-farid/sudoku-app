using Sudoku.Core.Domain.Model;
using Sudoku.Core.Application.Commands;

namespace Sudoku.Tests.Unit.Commands;

[Trait("Category", "Unit")]
public class CommandParserTests
{
    [Fact]
    public void TryParse_PlaceText_ReturnsPositionAndNumber()
    {
        Assert.True(CommandParser.TryParse("B3 7", out var cmd, out _));
        var place = Assert.IsType<PlaceCommand>(cmd);
        Assert.Equal(new Position(1, 2), place.Position);
        Assert.Equal(7, place.Number);
    }

    [Fact]
    public void TryParse_ClearText_ReturnsPosition()
    {
        Assert.True(CommandParser.TryParse("C5 CLEAR", out var cmd, out _));
        var clear = Assert.IsType<ClearCommand>(cmd);
        Assert.Equal(new Position(2, 4), clear.Position);
    }

    [Theory]
    [InlineData("hint", typeof(HintCommand))]
    [InlineData("HINT", typeof(HintCommand))]
    [InlineData("check", typeof(CheckCommand))]
    [InlineData("Check", typeof(CheckCommand))]
    [InlineData("quit", typeof(QuitCommand))]
    [InlineData("QUIT", typeof(QuitCommand))]
    public void TryParse_SimpleCommands(string input, Type expected)
    {
        Assert.True(CommandParser.TryParse(input, out var cmd, out _));
        Assert.IsType(expected, cmd);
    }

    [Fact]
    public void TryParse_UnrecognizedText_ReturnsError()
    {
        Assert.False(CommandParser.TryParse("unrecognized", out _, out var error));
        Assert.Equal(CommandParseError.UnrecognizedCommand, error);
    }

    [Fact]
    public void TryParse_OutOfRangeRow_ReturnsTypedError()
    {
        Assert.False(CommandParser.TryParse("Z3 7", out _, out var error));
        Assert.Equal(CommandParseError.RowOutOfRange, error);
    }

    [Fact]
    public void TryParse_OutOfColumnRow_ReturnsTypedError()
    {
        Assert.False(CommandParser.TryParse("A10 7", out _, out var error));
        Assert.Equal(CommandParseError.ColumnOutOfRange, error);
    }

    [Fact]
    public void TryParse_EmptyInputReturnsCommandRequired()
    {
        Assert.False(CommandParser.TryParse("  ", out _, out var error));
        Assert.Equal(CommandParseError.CommandRequired, error);
    }

    [Fact]
    public void TryParse_SuccessReturnsNoError()
    {
        Assert.True(CommandParser.TryParse("hint", out _, out var error));
        Assert.Equal(CommandParseError.None, error);
    }
}
