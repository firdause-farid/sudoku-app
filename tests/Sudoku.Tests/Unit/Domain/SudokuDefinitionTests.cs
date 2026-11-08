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

    [Fact]
    public void Positions_UseTheDefinitionsOwnRowLabels()
    {
        var definition = new SudokuDefinition(
            "custom-labels", 6, 2, 3,
            ["1", "2", "3", "4", "5", "6"],
            ["P", "Q", "R", "S", "T", "U"],
            8, 18);

        Assert.Equal("Q2", definition.FormatPosition(new Position(1, 1)));
        Assert.True(Position.TryParse("u6", definition, out Position position, out _));
        Assert.Equal(new Position(5, 5), position);
        Assert.False(Position.TryParse("A1", definition, out _, out PositionParseError error));
        Assert.Equal(PositionParseError.RowOutOfRange, error);
    }

    [Fact]
    public void PrefilledCellCountRule_IsDerivedFromTheDefinition()
    {
        SudokuDefinition definition = TestSudokuDefinitions.Mini6x6;

        Assert.Equal(36, definition.CellCount);
        Assert.Equal(35, definition.MaximumPrefilledCellCount);
        Assert.False(definition.IsValidPrefilledCellCount(definition.MinimumPrefilledCellCount - 1));
        Assert.True(definition.IsValidPrefilledCellCount(definition.MinimumPrefilledCellCount));
        Assert.True(definition.IsValidPrefilledCellCount(definition.MaximumPrefilledCellCount));
        Assert.False(definition.IsValidPrefilledCellCount(definition.CellCount));
    }

    [Fact]
    public void EnsureValidPrefilledCellCount_OutOfRange_ThrowsWithParameterName()
    {
        SudokuDefinition definition = TestSudokuDefinitions.Mini6x6;

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => definition.EnsureValidPrefilledCellCount(36, "count"));

        Assert.Equal("count", exception.ParamName);
        definition.EnsureValidPrefilledCellCount(18, "count");
    }

    [Fact]
    public void Constructor_DefaultPrefilledCountOutsideTheRule_Throws()
    {
        string[] symbols = ["1", "2", "3", "4", "5", "6"];
        string[] labels = ["A", "B", "C", "D", "E", "F"];

        Assert.Throws<ArgumentOutOfRangeException>(() => new SudokuDefinition("x", 6, 2, 3, symbols, labels, 8, 7));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SudokuDefinition("x", 6, 2, 3, symbols, labels, 8, 36));
    }
}
