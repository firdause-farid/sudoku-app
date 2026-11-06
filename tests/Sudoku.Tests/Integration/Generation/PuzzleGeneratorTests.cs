using Sudoku.Core.Domain.Model;
using Sudoku.Core.Domain.Generation;
using Sudoku.Core.Domain.Validation;
using Sudoku.Core.Domain.Randomness;
using Sudoku.Tests.Fakes;

namespace Sudoku.Tests.Integration.Generation;

[Trait("Category", "Integration")]
public class PuzzleGeneratorTests
{
    [Theory]
    [InlineData(16)]
    [InlineData(81)]
    public void Generate_PrefilledCountOutsideSupportedRange_Throws(int prefilledCount)
    {
        var generator = CreateGenerator(123);

        Assert.Throws<ArgumentOutOfRangeException>(() => generator.Generate(prefilledCount));
    }

    [Fact]
    public void Generate_With30PrefilledCells_ReturnsExactly30PrefilledCells()
    {
        var generator = CreateGenerator(123);
        var (puzzle, _) = generator.Generate(30);

        int prefilledCount = 0;
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                if (puzzle.IsPrefilled(new Position(r, c)))
                    prefilledCount++;

        Assert.Equal(30, prefilledCount);
    }

    [Fact]
    public void Generate_80PrefilledCells_ReturnsPlayablePuzzleWithOneEmptyCell()
    {
        var generator = CreateGenerator(321);

        var (puzzle, _) = generator.Generate(80);

        int prefilledCount = 0;
        int emptyCount = 0;
        for (int row = 0; row < 9; row++)
            for (int column = 0; column < 9; column++)
            {
                if (puzzle.IsPrefilled(new Position(row, column)))
                    prefilledCount++;
                else if (puzzle.GetValue(new Position(row, column)) is null)
                    emptyCount++;
            }

        Assert.Equal(80, prefilledCount);
        Assert.Equal(1, emptyCount);
    }

    [Fact]
    public void Generate_PuzzleHasUniqueSolution()
    {
        var generator = CreateGenerator(456);
        var (puzzle, solution) = generator.Generate(30);

        int count = new SudokuSolutionCounter().CountSolutions(puzzle.ToValueArray(), 2);
        Assert.Equal(1, count);

        // Solution grid itself must be valid.
        var solutionGrid = new Grid();
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                solutionGrid.SetValue(new Position(r, c), solution[r, c]);
        // Fill the empty grid with the solution so the validator can check it.
        Assert.True(new SudokuValidator().Validate(solutionGrid).IsValid);
    }

    [Fact]
    public void Generate_PrefilledCellsMatchSolution()
    {
        var generator = CreateGenerator(789);
        var (puzzle, solution) = generator.Generate(30);

        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
            {
                var v = puzzle.GetValue(new Position(r, c));
                if (v.HasValue)
                    Assert.Equal(solution[r, c], v.Value);
            }
    }

    [Fact]
    public void Generate_UsesInjectedSolutionGeneratorAndSolutionCounter()
    {
        var solutionGenerator = new RecordingSolutionGenerator();
        var solutionCounter = new RecordingSolutionCounter();
        var generator = new PuzzleGenerator(new SeededRandomSource(654), solutionGenerator, solutionCounter);

        generator.Generate(80);

        Assert.True(solutionCounter.UniqueSolutionChecks > 0);
        Assert.Equal(1, solutionGenerator.SolutionGenerations);
    }

    [Fact]
    public void Generate_SameSeedProducesSamePuzzleAndSolution()
    {
        var first = CreateGenerator(987).Generate(30);
        var second = CreateGenerator(987).Generate(30);

        for (int row = 0; row < 9; row++)
            for (int column = 0; column < 9; column++)
            {
                var position = new Position(row, column);
                Assert.Equal(first.Puzzle.GetValue(position), second.Puzzle.GetValue(position));
                Assert.Equal(first.Puzzle.IsPrefilled(position), second.Puzzle.IsPrefilled(position));
                Assert.Equal(first.Solution[row, column], second.Solution[row, column]);
            }
    }

    private static PuzzleGenerator CreateGenerator(int seed) =>
        new(new SeededRandomSource(seed), new SudokuSolutionGenerator(), new SudokuSolutionCounter());

    private sealed class RecordingSolutionGenerator : ISudokuSolutionGenerator
    {
        private readonly ISudokuSolutionGenerator _inner = new SudokuSolutionGenerator();

        public int SolutionGenerations { get; private set; }

        public int[,] GenerateSolution(IRandomSource random)
        {
            SolutionGenerations++;
            return _inner.GenerateSolution(random);
        }
    }

    private sealed class RecordingSolutionCounter : ISudokuSolutionCounter
    {
        private readonly SudokuSolutionCounter _inner = new();

        public int UniqueSolutionChecks { get; private set; }

        public bool HasUniqueSolution(int?[,] grid)
        {
            UniqueSolutionChecks++;
            return _inner.HasUniqueSolution(grid);
        }

    }
}

