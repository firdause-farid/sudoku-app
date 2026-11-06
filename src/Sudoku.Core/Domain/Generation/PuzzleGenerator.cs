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

    public (Grid Puzzle, int[,] Solution) Generate(int prefilledCount = PuzzleGenerationSettings.RequiredPrefilledCellCount)
        => Generate(prefilledCount, SudokuDefinition.Classic);

    public (Grid Puzzle, int[,] Solution) Generate(int prefilledCount, SudokuDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        int cellCount = definition.GridSize * definition.GridSize;
        if (prefilledCount < definition.MinimumPrefilledCellCount || prefilledCount >= cellCount)
            throw new ArgumentOutOfRangeException(nameof(prefilledCount), $"Prefilled cells for '{definition.Id}' must be between {definition.MinimumPrefilledCellCount} and {cellCount - 1}.");

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

        var values = new int?[definition.GridSize, definition.GridSize];
        var prefilledMask = new bool[definition.GridSize, definition.GridSize];
        for (int r = 0; r < definition.GridSize; r++)
            for (int c = 0; c < definition.GridSize; c++)
            {
                values[r, c] = solution[r, c];
                prefilledMask[r, c] = true;
            }

        int remainingPrefilledCount = definition.GridSize * definition.GridSize;

        var positions = new List<(int r, int c)>();
        for (int r = 0; r < definition.GridSize; r++)
            for (int c = 0; c < definition.GridSize; c++)
                positions.Add((r, c));

        _random.Shuffle(positions);

        foreach (var (r, c) in positions)
        {
            if (remainingPrefilledCount <= prefilledCount)
                break;

            // Tentatively remove.
            int? saved = values[r, c];
            values[r, c] = null;
            prefilledMask[r, c] = false;

            // Keep removal only if puzzle stays uniquely solvable.
            if (!_solutionCounter.HasUniqueSolution(values, definition))
            {
                values[r, c] = saved;
                prefilledMask[r, c] = true;
                continue;
            }

            remainingPrefilledCount--;
        }

        if (remainingPrefilledCount != prefilledCount)
            return null;

        var puzzle = Grid.FromArrays(values, prefilledMask, definition);
        return (puzzle, solution);
    }

}
