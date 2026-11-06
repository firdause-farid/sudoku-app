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
        SudokuDefinition? definition = null,
        int? prefilledCount = null)
    {
        _puzzleGenerator = puzzleGenerator ?? throw new ArgumentNullException(nameof(puzzleGenerator));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _random = random ?? throw new ArgumentNullException(nameof(random));
        _definition = definition ?? SudokuDefinition.Classic;
        _prefilledCount = prefilledCount ?? _definition.DefaultPrefilledCellCount;
        int cellCount = _definition.GridSize * _definition.GridSize;
        if (_prefilledCount < _definition.MinimumPrefilledCellCount || _prefilledCount >= cellCount)
            throw new ArgumentOutOfRangeException(nameof(prefilledCount), "Prefilled-cell count must meet the definition's minimum and leave at least one empty cell.");
    }

    public ISudokuGame Create()
    {
        var (puzzle, solution) = _puzzleGenerator.Generate(_prefilledCount, _definition);
        return new SudokuGame(puzzle, solution, _validator, _random);
    }
}
