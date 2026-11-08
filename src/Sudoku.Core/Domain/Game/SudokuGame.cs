using Sudoku.Core.Domain.Model;
using Sudoku.Core.Domain.Randomness;
using Sudoku.Core.Domain.Validation;
using Sudoku.Core.Domain.Game.Results;

namespace Sudoku.Core.Domain.Game;

/// <summary>
/// Pure Sudoku game state and rules (no I/O).
/// </summary>
public sealed class SudokuGame : ISudokuGame
{
    private readonly Grid _grid;
    private readonly IGridView _gridView;
    private readonly int[,] _solution;
    private readonly IValidator _validator;
    private readonly IRandomSource _random;

    /// <summary>
    /// Creates a game from a puzzle and its solution. The game keeps its own copy of both,
    /// so later changes by the caller do not affect play or hints.
    /// </summary>
    public SudokuGame(Grid puzzle, int[,] solution, IValidator validator, IRandomSource random)
    {
        ArgumentNullException.ThrowIfNull(puzzle);
        ArgumentNullException.ThrowIfNull(solution);
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _random = random ?? throw new ArgumentNullException(nameof(random));

        if (!_validator.Validate(puzzle).IsValid)
            throw new ArgumentException("Puzzle must satisfy all Sudoku rules.", nameof(puzzle));

        Grid solutionGrid = BuildSolutionGrid(puzzle, solution);
        if (!_validator.Validate(solutionGrid).IsValid)
            throw new ArgumentException("Solution must satisfy all Sudoku rules.", nameof(solution));

        _grid = puzzle.Clone();
        _gridView = new ReadOnlyGridView(_grid);
        _solution = (int[,])solution.Clone();
    }

    /// <summary>
    /// Returns a read-only live view of the current grid without copying it.
    /// </summary>
    public IGridView GetGridView() => _gridView;

    public MoveResult Place(Position position, int number)
    {
        if (!_grid.Definition.IsValueInRange(number))
            return new MoveResult(MoveStatus.InvalidNumber);

        if (_grid.IsPrefilled(position))
            return new MoveResult(MoveStatus.PrefilledCell);

        _grid.SetValue(position, number);
        return new MoveResult(MoveStatus.Accepted);
    }

    public MoveResult Clear(Position position)
    {
        if (_grid.IsPrefilled(position))
            return new MoveResult(MoveStatus.PrefilledCell);

        _grid.Clear(position);
        return new MoveResult(MoveStatus.Accepted);
    }

    public HintResult? Hint()
    {
        List<Position> emptyPositions = FindEmptyPositions();
        if (emptyPositions.Count == 0)
            return null;

        Position chosen = emptyPositions[_random.Next(emptyPositions.Count)];
        return new HintResult(chosen, _solution[chosen.Row, chosen.Column]);
    }

    public ValidationResult Check() => _validator.Validate(_grid);

    public bool IsComplete() => _grid.IsComplete() && _validator.Validate(_grid).IsValid;

    private List<Position> FindEmptyPositions()
    {
        var emptyPositions = new List<Position>();
        for (int row = 0; row < _grid.Definition.GridSize; row++)
            for (int column = 0; column < _grid.Definition.GridSize; column++)
            {
                var position = new Position(row, column);
                if (_grid.GetValue(position) is null)
                    emptyPositions.Add(position);
            }

        return emptyPositions;
    }

    private static Grid BuildSolutionGrid(Grid puzzle, int[,] solution)
    {
        SudokuDefinition definition = puzzle.Definition;
        if (solution.GetLength(0) != definition.GridSize || solution.GetLength(1) != definition.GridSize)
            throw new ArgumentException($"Solution array must be {definition.GridSize}x{definition.GridSize}.", nameof(solution));

        var solutionGrid = new Grid(definition);
        for (int row = 0; row < definition.GridSize; row++)
            for (int column = 0; column < definition.GridSize; column++)
            {
                var position = new Position(row, column);
                int value = solution[row, column];
                if (!definition.IsValueInRange(value))
                    throw new ArgumentException($"Solution values must be between 1 and {definition.GridSize}.", nameof(solution));
                if (puzzle.IsPrefilled(position) && puzzle.GetValue(position) != value)
                    throw new ArgumentException("Solution must match every pre-filled puzzle cell.", nameof(solution));

                solutionGrid.SetValue(position, value);
            }

        return solutionGrid;
    }

    private sealed class ReadOnlyGridView(Grid grid) : IGridView
    {
        public SudokuDefinition Definition => grid.Definition;

        public int? GetValue(Position position) => grid.GetValue(position);

        public bool IsPrefilled(Position position) => grid.IsPrefilled(position);

        public bool IsComplete() => grid.IsComplete();
    }
}
