using Sudoku.Core.Domain.Model;
using Sudoku.Tests.Fakes;

namespace Sudoku.Tests.Unit.Domain;

[Trait("Category", "Unit")]
public class GridTests
{
    [Fact]
    public void NewGrid_IsEmptyAndIncomplete()
    {
        var grid = new Grid(SudokuDefinition.Classic);
        Assert.False(grid.IsComplete());
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                Assert.Null(grid.GetValue(new Position(r, c)));
    }

    [Fact]
    public void SetValue_AndClear_WorkOnNonPrefilledCell()
    {
        var grid = new Grid(SudokuDefinition.Classic);
        var pos = new Position(0, 0);
        grid.SetValue(pos, 5);
        Assert.Equal(5, grid.GetValue(pos));
        grid.Clear(pos);
        Assert.Null(grid.GetValue(pos));
    }

    [Fact]
    public void SetValue_OnPrefilledCell_Throws()
    {
        var values = new int?[9, 9];
        var prefilledMask = new bool[9, 9];
        values[0, 0] = 5;
        prefilledMask[0, 0] = true;
        var grid = Grid.FromArrays(values, prefilledMask, SudokuDefinition.Classic);

        Assert.True(grid.IsPrefilled(new Position(0, 0)));
        Assert.Throws<InvalidOperationException>(() => grid.SetValue(new Position(0, 0), 7));
    }

    [Fact]
    public void Cell_PrefilledWithoutValue_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Cell(null, isPrefilled: true));
    }

    [Fact]
    public void Clone_IsIndependentCopy()
    {
        var grid = new Grid(SudokuDefinition.Classic);
        grid.SetValue(new Position(0, 0), 5);
        var clone = grid.Clone();
        clone.Clear(new Position(0, 0));
        Assert.Equal(5, grid.GetValue(new Position(0, 0)));
        Assert.Null(clone.GetValue(new Position(0, 0)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    public void SetValue_ValueOutOfRange_ThrowsAndKeepsValue(int value)
    {
        var grid = new Grid(SudokuDefinition.Classic);
        var position = new Position(0, 0);
        grid.SetValue(position, 3);

        Assert.Throws<ArgumentOutOfRangeException>(() => grid.SetValue(position, value));
        Assert.Equal(3, grid.GetValue(position));
    }

    [Fact]
    public void SetValue_UsesTheGridsOwnRange()
    {
        var grid = new Grid(TestSudokuDefinitions.Mini6x6);

        grid.SetValue(new Position(0, 0), 6);

        Assert.Throws<ArgumentOutOfRangeException>(() => grid.SetValue(new Position(0, 0), 7));
    }

    [Fact]
    public void FromArrays_ValueOutOfRange_Throws()
    {
        var values = new int?[9, 9];
        values[4, 4] = 10;

        Assert.Throws<ArgumentOutOfRangeException>(
            () => Grid.FromArrays(values, new bool[9, 9], SudokuDefinition.Classic));
    }
}
