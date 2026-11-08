using Sudoku.Core.Domain.Model;

namespace Sudoku.Tests.Unit.Domain;

[Trait("Category", "Unit")]
public class CellTests
{
    [Fact]
    public void NewCell_WithValueAndPrefilledFlag_ExposesBoth()
    {
        var cell = new Cell(7, isPrefilled: true, definition: SudokuDefinition.Classic);

        Assert.Equal(7, cell.Value);
        Assert.True(cell.IsPrefilled);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    public void Constructor_ValueOutOfRange_Throws(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Cell(value, isPrefilled: false, definition: SudokuDefinition.Classic));
    }

    [Fact]
    public void Constructor_PrefilledCellWithoutValue_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Cell(null, isPrefilled: true, definition: SudokuDefinition.Classic));
    }

    [Fact]
    public void SetValue_OnPrefilledCell_Throws()
    {
        var cell = new Cell(5, isPrefilled: true, definition: SudokuDefinition.Classic);

        Assert.Throws<InvalidOperationException>(() => cell.SetValue(6));
        Assert.Equal(5, cell.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    public void SetValue_OutOfRange_ThrowsAndKeepsValue(int value)
    {
        var cell = new Cell(3, isPrefilled: false, definition: SudokuDefinition.Classic);

        Assert.Throws<ArgumentOutOfRangeException>(() => cell.SetValue(value));
        Assert.Equal(3, cell.Value);
    }

    [Fact]
    public void SetValue_Null_ClearsEditableCell()
    {
        var cell = new Cell(3, isPrefilled: false, definition: SudokuDefinition.Classic);

        cell.SetValue(null);

        Assert.Null(cell.Value);
    }
}
