using Sudoku.Core.Domain.Model;

namespace Sudoku.Tests.Unit.Domain;

[Trait("Category", "Unit")]
public class CellTests
{
    [Fact]
    public void NewCell_WithValueAndPrefilledFlag_ExposesBoth()
    {
        var cell = new Cell(7, isPrefilled: true);

        Assert.Equal(7, cell.Value);
        Assert.True(cell.IsPrefilled);
    }

    [Fact]
    public void Constructor_PrefilledCellWithoutValue_Throws()
    {
        Assert.Throws<ArgumentException>(() => new Cell(null, isPrefilled: true));
    }

    [Fact]
    public void SetValue_OnPrefilledCell_Throws()
    {
        var cell = new Cell(5, isPrefilled: true);

        Assert.Throws<InvalidOperationException>(() => cell.SetValue(6));
        Assert.Equal(5, cell.Value);
    }

    [Fact]
    public void SetValue_Null_ClearsEditableCell()
    {
        var cell = new Cell(3, isPrefilled: false);

        cell.SetValue(null);

        Assert.Null(cell.Value);
    }
}
