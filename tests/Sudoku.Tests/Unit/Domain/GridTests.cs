using Sudoku.Core.Domain.Model;

namespace Sudoku.Tests.Unit.Domain;

[Trait("Category", "Unit")]
public class GridTests
{
    [Fact]
    public void NewGrid_IsEmptyAndIncomplete()
    {
        var grid = new Grid();
        Assert.False(grid.IsComplete());
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                Assert.Null(grid.GetValue(new Position(r, c)));
    }

    [Fact]
    public void SetValue_AndClear_WorkOnNonPrefilledCell()
    {
        var grid = new Grid();
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
        var grid = Grid.FromArrays(values, prefilledMask);

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
        var grid = new Grid();
        grid.SetValue(new Position(0, 0), 5);
        var clone = grid.Clone();
        clone.Clear(new Position(0, 0));
        Assert.Equal(5, grid.GetValue(new Position(0, 0)));
        Assert.Null(clone.GetValue(new Position(0, 0)));
    }
}
