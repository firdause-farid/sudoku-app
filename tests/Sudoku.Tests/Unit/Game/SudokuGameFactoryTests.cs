using Sudoku.Core.Domain.Game;
using Sudoku.Core.Domain.Generation;
using Sudoku.Core.Domain.Model;
using Sudoku.Core.Domain.Validation;
using Sudoku.Tests.Fakes;

namespace Sudoku.Tests.Unit.Game;

[Trait("Category", "Unit")]
public class SudokuGameFactoryTests
{
    [Fact]
    public void Create_RequestsTheRequiredNumberOfPrefilledCells()
    {
        var generator = new RecordingPuzzleGenerator(CreateInnerGenerator(1));
        var factory = new SudokuGameFactory(generator, new SudokuValidator(), new SeededRandomSource(1), SudokuDefinition.Classic);

        factory.Create();

        Assert.Equal(new[] { SudokuDefinition.Classic.DefaultPrefilledCellCount }, generator.RequestedCounts);
    }

    [Fact]
    public void Create_ReturnsGameStartingFromTheGeneratedPuzzle()
    {
        var generator = new RecordingPuzzleGenerator(CreateInnerGenerator(2));
        var factory = new SudokuGameFactory(generator, new SudokuValidator(), new SeededRandomSource(2), SudokuDefinition.Classic);

        ISudokuGame game = factory.Create();

        Grid puzzle = generator.LastPuzzle!;
        for (int row = 0; row < 9; row++)
            for (int column = 0; column < 9; column++)
            {
                var position = new Position(row, column);
                Assert.Equal(puzzle.GetValue(position), game.GetGridView().GetValue(position));
                Assert.Equal(puzzle.IsPrefilled(position), game.GetGridView().IsPrefilled(position));
            }
    }

    [Fact]
    public void Create_CalledTwice_GeneratesANewPuzzleEachTime()
    {
        var generator = new RecordingPuzzleGenerator(CreateInnerGenerator(3));
        var factory = new SudokuGameFactory(generator, new SudokuValidator(), new SeededRandomSource(3), SudokuDefinition.Classic);

        factory.Create();
        factory.Create();

        Assert.Equal(2, generator.RequestedCounts.Count);
    }

    [Fact]
    public void Constructor_RejectsMissingDependencies()
    {
        IPuzzleGenerator generator = CreateInnerGenerator(4);
        var validator = new SudokuValidator();
        var random = new SeededRandomSource(4);

        Assert.Throws<ArgumentNullException>(() => new SudokuGameFactory(null!, validator, random, SudokuDefinition.Classic));
        Assert.Throws<ArgumentNullException>(() => new SudokuGameFactory(generator, null!, random, SudokuDefinition.Classic));
        Assert.Throws<ArgumentNullException>(() => new SudokuGameFactory(generator, validator, null!, SudokuDefinition.Classic));
    }

    private static IPuzzleGenerator CreateInnerGenerator(int seed) =>
        new PuzzleGenerator(new SeededRandomSource(seed), new SudokuSolutionGenerator(), new SudokuSolutionCounter());

    private sealed class RecordingPuzzleGenerator(IPuzzleGenerator inner) : IPuzzleGenerator
    {
        public List<int> RequestedCounts { get; } = new();
        public Grid? LastPuzzle { get; private set; }

        public (Grid Puzzle, int[,] Solution) Generate(int prefilledCount, SudokuDefinition definition)
        {
            RequestedCounts.Add(prefilledCount);
            var generated = inner.Generate(prefilledCount, definition);
            LastPuzzle = generated.Puzzle;
            return generated;
        }
    }
}
