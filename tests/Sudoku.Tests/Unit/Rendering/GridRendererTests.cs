using Sudoku.Core.Domain.Model;
using Sudoku.Core.Presentation.Messages;
using Sudoku.Core.Presentation.Rendering;

namespace Sudoku.Tests.Unit.Rendering;

[Trait("Category", "Unit")]
public class GridRendererTests
{
    [Fact]
    public void Render_EmptyGrid_MatchesFormat()
    {
        var grid = new Grid();
        var expectedLines = new[] { $"    {UserMessages.GridHeader}" }
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
        var grid = Grid.FromArrays(values, prefilledMask);

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
        var grid = Grid.FromArrays(values, prefilledMask);

        string rendered = new GridRenderer(new EnhancedGridStyle()).Render(grid);

        Assert.Contains($"    | \u001b[2m{UserMessages.GridHeader}\u001b[0m", rendered);
        Assert.Contains(UserMessages.GridHeaderSeparator, rendered);
        Assert.Contains("  \u001b[36mA\u001b[0m | \u001b[1m\u001b[93m5\u001b[0m 6", rendered);
    }
}
