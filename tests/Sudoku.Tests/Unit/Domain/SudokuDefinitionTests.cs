using Sudoku.Core.Domain.Model;
using Sudoku.Tests.Fakes;

namespace Sudoku.Tests.Unit.Domain;

[Trait("Category", "Unit")]
public class SudokuDefinitionTests
{
    [Fact]
    public void TryParsePosition_UsesConfiguredRowsAndColumns()
    {
        SudokuDefinition definition = TestSudokuDefinitions.Mini6x6;

        bool parsed = Position.TryParse("f6", definition, out Position position, out PositionParseError error);

        Assert.True(parsed);
        Assert.Equal(PositionParseError.None, error);
        Assert.Equal(new Position(5, 5), position);
        Assert.Equal("F6", definition.FormatPosition(position));
    }

    [Fact]
    public void TryParseValue_UsesConfiguredSymbols()
    {
        SudokuDefinition definition = TestSudokuDefinitions.Mini6x6;

        Assert.True(definition.TryParseValue("6", out int value));
        Assert.Equal(6, value);
        Assert.Equal("6", definition.FormatValue(value));
        Assert.False(definition.TryParseValue("9", out _));
    }
}
