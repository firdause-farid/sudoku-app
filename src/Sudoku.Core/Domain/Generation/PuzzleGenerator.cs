using Sudoku.Core.Domain.Model;
using Sudoku.Core.Domain.Randomness;

namespace Sudoku.Core.Domain.Generation;

/// <summary>
/// Generates Sudoku puzzles with a unique solution.
/// </summary>
public sealed class PuzzleGenerator : IPuzzleGenerator
{
    private const int MinimumUniquePrefilledCells = 17;
    private const int MaximumPlayablePrefilledCells = 80;
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
    {
        if (prefilledCount < MinimumUniquePrefilledCells || prefilledCount > MaximumPlayablePrefilledCells)
            throw new ArgumentOutOfRangeException(nameof(prefilledCount), $"Prefilled cells must be between {MinimumUniquePrefilledCells} and {MaximumPlayablePrefilledCells} for a playable, uniquely solvable Sudoku puzzle.");

        for (int attempt = 0; attempt < MaximumGenerationAttempts; attempt++)
        {
            var generated = TryGenerate(prefilledCount);
            if (generated is not null)
                return generated.Value;
        }

        throw new InvalidOperationException($"Could not generate a uniquely solvable puzzle with exactly {prefilledCount} prefilled cells after {MaximumGenerationAttempts} attempts.");
    }

    private (Grid Puzzle, int[,] Solution)? TryGenerate(int prefilledCount)
    {
        int[,] solution = _solutionGenerator.GenerateSolution(_random);

        var values = new int?[SudokuRules.GridSize, SudokuRules.GridSize];
        var prefilledMask = new bool[SudokuRules.GridSize, SudokuRules.GridSize];
        for (int r = 0; r < SudokuRules.GridSize; r++)
            for (int c = 0; c < SudokuRules.GridSize; c++)
            {
                values[r, c] = solution[r, c];
                prefilledMask[r, c] = true;
            }

        int remainingPrefilledCount = SudokuRules.GridSize * SudokuRules.GridSize;

        var positions = new List<(int r, int c)>();
        for (int r = 0; r < SudokuRules.GridSize; r++)
            for (int c = 0; c < SudokuRules.GridSize; c++)
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
            if (!_solutionCounter.HasUniqueSolution(values))
            {
                values[r, c] = saved;
                prefilledMask[r, c] = true;
                continue;
            }

            remainingPrefilledCount--;
        }

        if (remainingPrefilledCount != prefilledCount)
            return null;

        var puzzle = Grid.FromArrays(values, prefilledMask);
        return (puzzle, solution);
    }

}
