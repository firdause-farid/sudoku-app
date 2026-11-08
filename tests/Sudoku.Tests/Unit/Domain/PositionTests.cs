using Sudoku.Core.Domain.Model;

namespace Sudoku.Tests.Unit.Domain;

[Trait("Category", "Unit")]
public class PositionTests
{
    [Theory]
    [InlineData("A1", 0, 0)]
    [InlineData("I9", 8, 8)]
    [InlineData("B3", 1, 2)]
    [InlineData("b3", 1, 2)]
    [InlineData("  c5 ", 2, 4)]
    public void TryParse_ValidPosition_ReturnsExpected(string text, int row, int col)
    {
        bool parsed = Position.TryParse(text, SudokuDefinition.Classic, out Position position, out PositionParseError error);

        Assert.True(parsed);
        Assert.Equal(PositionParseError.None, error);
        Assert.Equal(row, position.Row);
        Assert.Equal(col, position.Column);
    }

    [Fact]
    public void FormatPosition_ReturnsRowLabelAndColumnNumber()
    {
        Assert.Equal("B3", SudokuDefinition.Classic.FormatPosition(new Position(1, 2)));
    }

    [Theory]
    [InlineData(null, PositionParseError.Required)]
    [InlineData("", PositionParseError.Required)]
    [InlineData("   ", PositionParseError.Required)]
    [InlineData("Z1", PositionParseError.RowOutOfRange)]
    [InlineData("1A", PositionParseError.RowOutOfRange)]
    [InlineData("A0", PositionParseError.ColumnOutOfRange)]
    [InlineData("A10", PositionParseError.ColumnOutOfRange)]
    [InlineData("A+3", PositionParseError.InvalidFormat)]
    [InlineData("A-1", PositionParseError.InvalidFormat)]
    [InlineData("A３", PositionParseError.InvalidFormat)]
    public void TryParse_InvalidPosition_ReportsTypedError(string? text, PositionParseError expectedError)
    {
        bool parsed = Position.TryParse(text, SudokuDefinition.Classic, out _, out PositionParseError error);

        Assert.False(parsed);
        Assert.Equal(expectedError, error);
    }
}
