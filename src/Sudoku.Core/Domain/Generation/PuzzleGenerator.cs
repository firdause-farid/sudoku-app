using Sudoku.Core.Domain.Model;
using Sudoku.Core.Domain.Randomness;

namespace Sudoku.Core.Domain.Generation;

/// <summary>
/// Generates Sudoku puzzles with a unique solution.
/// </summary>
public sealed class PuzzleGenerator : IPuzzleGenerator
{
    private const int MaximumGenerationAttempts = 100;
    private readonly IRandomSource _random;
    private readonly ISudokuSolutionGenerator _solutionGenerator;
    private readonly ISudokuSolutionCounter _solutionCounter;

    public PuzzleGenerator(
        IRandomSource random,
        ISudokuSolutionGenerator solutionGenerator,
        ISudokuSolutionCounter solutionCounter)
    {
        _random = random ?? throw new ArgumentNullException(nameof(random));
        _solutionGenerator = solutionGenerator ?? throw new ArgumentNullException(nameof(solutionGenerator));
        _solutionCounter = solutionCounter ?? throw new ArgumentNullException(nameof(solutionCounter));
    }

    public (Grid Puzzle, int[,] Solution) Generate(int prefilledCount, SudokuDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        definition.EnsureValidPrefilledCellCount(prefilledCount, nameof(prefilledCount));

        for (int attempt = 0; attempt < MaximumGenerationAttempts; attempt++)
        {
            var generated = TryGenerate(prefilledCount, definition);
            if (generated is not null)
                return generated.Value;
        }

        throw new InvalidOperationException($"Could not generate a uniquely solvable '{definition.Id}' puzzle with exactly {prefilledCount} prefilled cells after {MaximumGenerationAttempts} attempts.");
    }

    private (Grid Puzzle, int[,] Solution)? TryGenerate(int prefilledCount, SudokuDefinition definition)
    {
        int[,] solution = _solutionGenerator.GenerateSolution(_random, definition);
        int?[,] values = ToNullable(solution, definition);
        bool[,] prefilledMask = CreateMask(definition.GridSize, filled: true);

        int remainingPrefilledCount = RemoveCluesWhileUnique(values, prefilledMask, prefilledCount, definition);
        if (remainingPrefilledCount != prefilledCount)
            return null;

        return (Grid.FromArrays(values, prefilledMask, definition), solution);
    }

    /// <summary>
    /// Visits every cell in random order and blanks it if the puzzle still has exactly one solution,
    /// until only <paramref name="targetCount"/> cells remain. Returns how many cells remain.
    /// </summary>
    private int RemoveCluesWhileUnique(int?[,] values, bool[,] prefilledMask, int targetCount, SudokuDefinition definition)
    {
        int remainingPrefilledCount = definition.CellCount;
        foreach (var (row, column) in CreateShuffledPositions(definition))
        {
            if (remainingPrefilledCount <= targetCount)
                break;

            if (TryRemoveClue(values, prefilledMask, row, column, definition))
                remainingPrefilledCount--;
        }

        return remainingPrefilledCount;
    }

    /// <summary>Blanks one cell and keeps it blank only if the puzzle is still uniquely solvable.</summary>
    private bool TryRemoveClue(int?[,] values, bool[,] prefilledMask, int row, int column, SudokuDefinition definition)
    {
        int? saved = values[row, column];
        values[row, column] = null;
        prefilledMask[row, column] = false;

        if (_solutionCounter.HasUniqueSolution(values, definition))
            return true;

        values[row, column] = saved;
        prefilledMask[row, column] = true;
        return false;
    }

    private List<(int Row, int Column)> CreateShuffledPositions(SudokuDefinition definition)
    {
        var positions = new List<(int Row, int Column)>();
        for (int row = 0; row < definition.GridSize; row++)
            for (int column = 0; column < definition.GridSize; column++)
                positions.Add((row, column));

        _random.Shuffle(positions);
        return positions;
    }

    private static int?[,] ToNullable(int[,] solution, SudokuDefinition definition)
    {
        var values = new int?[definition.GridSize, definition.GridSize];
        for (int row = 0; row < definition.GridSize; row++)
            for (int column = 0; column < definition.GridSize; column++)
                values[row, column] = solution[row, column];

        return values;
    }

    private static bool[,] CreateMask(int size, bool filled)
    {
        var mask = new bool[size, size];
        for (int row = 0; row < size; row++)
            for (int column = 0; column < size; column++)
                mask[row, column] = filled;

        return mask;
    }
}
