using Sudoku.Core.Domain.Generation;
using Sudoku.Core.Domain.Model;
using Sudoku.Core.Domain.Randomness;
using Sudoku.Core.Domain.Validation;

namespace Sudoku.Core.Domain.Game;

/// <summary>
/// Creates games with the configured validator and hint random source.
/// </summary>
public sealed class SudokuGameFactory : ISudokuGameFactory
{
    private readonly IValidator _validator;
    private readonly IRandomSource _random;
    private readonly IPuzzleGenerator _puzzleGenerator;
    private readonly SudokuDefinition _definition;
    private readonly int _prefilledCount;

    public SudokuGameFactory(
        IPuzzleGenerator puzzleGenerator,
        IValidator validator,
        IRandomSource random,
        SudokuDefinition definition,
        int? prefilledCount = null)
    {
        _puzzleGenerator = puzzleGenerator ?? throw new ArgumentNullException(nameof(puzzleGenerator));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _random = random ?? throw new ArgumentNullException(nameof(random));
        _definition = definition ?? throw new ArgumentNullException(nameof(definition));
        _prefilledCount = prefilledCount ?? _definition.DefaultPrefilledCellCount;
        _definition.EnsureValidPrefilledCellCount(_prefilledCount, nameof(prefilledCount));
    }

    public ISudokuGame Create()
    {
        var (puzzle, solution) = _puzzleGenerator.Generate(_prefilledCount, _definition);
        return new SudokuGame(puzzle, solution, _validator, _random);
    }
}
