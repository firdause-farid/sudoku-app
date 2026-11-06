using Sudoku.Core.Domain.Model;
using Sudoku.Core.Domain.Validation;

namespace Sudoku.Tests.Unit.Validation;

[Trait("Category", "Unit")]
public class SudokuValidatorTests
{
    [Fact]
    public void EmptyGrid_IsValid()
    {
        var result = new SudokuValidator().Validate(new Grid());
        Assert.True(result.IsValid);
    }

    [Fact]
    public void RowDuplicate_ReturnsStructuredIssue()
    {
        var values = new int?[9, 9];
        var prefilledMask = new bool[9, 9];
        values[0, 0] = 3;
        values[0, 1] = 3;
        var grid = Grid.FromArrays(values, prefilledMask);

        var result = new SudokuValidator().Validate(grid);
        Assert.False(result.IsValid);
        Assert.Equal(new ValidationIssue(ValidationIssueType.DuplicateRow, 3, 0), result.Issue);
    }

    [Fact]
    public void ColumnDuplicate_ReturnsStructuredIssue()
    {
        var values = new int?[9, 9];
        var prefilledMask = new bool[9, 9];
        values[0, 0] = 5;
        values[1, 0] = 5;
        var grid = Grid.FromArrays(values, prefilledMask);

        var result = new SudokuValidator().Validate(grid);
        Assert.False(result.IsValid);
        Assert.Equal(new ValidationIssue(ValidationIssueType.DuplicateColumn, 5, 0), result.Issue);
    }

    [Fact]
    public void BoxDuplicate_ReturnsStructuredIssue()
    {
        var values = new int?[9, 9];
        var prefilledMask = new bool[9, 9];
        values[0, 0] = 8;
        values[1, 1] = 8;
        var grid = Grid.FromArrays(values, prefilledMask);

        var result = new SudokuValidator().Validate(grid);
        Assert.False(result.IsValid);
        Assert.Equal(new ValidationIssue(ValidationIssueType.DuplicateSubgrid, 8), result.Issue);
    }

    [Fact]
    public void CompleteSolution_IsValid()
    {
        // A known valid solution.
        int[,] solution = new int[9, 9]
        {
            {5,3,4,6,7,8,9,1,2},
            {6,7,2,1,9,5,3,4,8},
            {1,9,8,3,4,2,5,6,7},
            {8,5,9,7,6,1,4,2,3},
            {4,2,6,8,5,3,7,9,1},
            {7,1,3,9,2,4,8,5,6},
            {9,6,1,5,3,7,2,8,4},
            {2,8,7,4,1,9,6,3,5},
            {3,4,5,2,8,6,1,7,9}
        };
        var values = new int?[9, 9];
        var prefilledMask = new bool[9, 9];
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
            {
                values[r, c] = solution[r, c];
                prefilledMask[r, c] = true;
            }
        var grid = Grid.FromArrays(values, prefilledMask);
        var result = new SudokuValidator().Validate(grid);
        Assert.True(result.IsValid);
    }
}
