using Sudoku.Core.Domain.Model;
using Sudoku.Core.Presentation.Messages;
using Sudoku.Core.Presentation.Rendering;
using Sudoku.Core.Presentation.Rendering.Styles;
using Sudoku.Tests.Fakes;

namespace Sudoku.Tests.Unit.Rendering;

[Trait("Category", "Unit")]
public class GridRendererTests
{
    [Fact]
    public void Render_EmptyGrid_MatchesFormat()
    {
        var grid = new Grid(SudokuDefinition.Classic);
        var expectedLines = new[] { "    1 2 3 4 5 6 7 8 9" }
            .Concat(Enumerable.Range(0, 9).Select(row =>
                $"  {(char)('A' + row)} {string.Join(" ", Enumerable.Repeat(UserMessages.EmptyCell, 9))}"));
        var expected = string.Join(Environment.NewLine, expectedLines);
        Assert.Equal(expected, new GridRenderer(new PlainGridStyle()).Render(grid));
    }

    [Fact]
    public void Render_PlainStyle_ShowsPrefilledAndPlayerValuesWithoutAnsiStyling()
    {
        var values = new int?[9, 9];
        var prefilledMask = new bool[9, 9];
        values[0, 0] = 5;
        prefilledMask[0, 0] = true;
        values[0, 1] = 6;
        var grid = Grid.FromArrays(values, prefilledMask, SudokuDefinition.Classic);

        string rendered = new GridRenderer(new PlainGridStyle()).Render(grid);

        Assert.Contains("  A 5 6 _ _ _ _ _ _ _", rendered);
        Assert.DoesNotContain("\u001b[", rendered);
    }

    [Fact]
    public void Render_WithUiEnhancements_StylesHeadersAndPrefilledCells()
    {
        var values = new int?[9, 9];
        var prefilledMask = new bool[9, 9];
        values[0, 0] = 5;
        prefilledMask[0, 0] = true;
        values[0, 1] = 6;
        var grid = Grid.FromArrays(values, prefilledMask, SudokuDefinition.Classic);

        string rendered = new GridRenderer(new EnhancedGridStyle()).Render(grid);

        Assert.Contains($"    | \u001b[2m1 2 3 4 5 6 7 8 9\u001b[0m", rendered);
        Assert.Contains("----+------------------", rendered);
        Assert.Contains("  \u001b[36mA\u001b[0m | \u001b[1m\u001b[93m5\u001b[0m 6", rendered);
    }

    [Fact]
    public void Render_UsesConfiguredGridSizeAndRowLabels()
    {
        SudokuDefinition definition = TestSudokuDefinitions.Mini6x6;
        var grid = new Grid(definition);

        string rendered = new GridRenderer(new PlainGridStyle()).Render(grid);

        Assert.StartsWith("    1 2 3 4 5 6", rendered);
        Assert.Contains("  A _ _ _ _ _ _", rendered);
        Assert.Contains("  F _ _ _ _ _ _", rendered);
        Assert.DoesNotContain("  G ", rendered);
    }

}
