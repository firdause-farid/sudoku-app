using Sudoku.Console.Adapters;

namespace Sudoku.Tests.Unit.Adapters;

[Trait("Category", "Unit")]
public class SystemRandomSourceTests
{
    [Fact]
    public void Next_ReturnsValuesBelowTheExclusiveMaximum()
    {
        var random = new SystemRandomSource();

        for (int i = 0; i < 200; i++)
            Assert.InRange(random.Next(5), 0, 4);
    }
}
