namespace Sudoku.Core.Domain.Model;

/// <summary>A Sudoku grid whose dimensions and symbols come from its definition.</summary>
public sealed class Grid : IGridView
{
    private readonly Cell[,] _cells;

    public Grid(SudokuDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        Definition = definition;
        _cells = new Cell[Definition.GridSize, Definition.GridSize];
        for (int row = 0; row < Definition.GridSize; row++)
            for (int column = 0; column < Definition.GridSize; column++)
                _cells[row, column] = new Cell(null, false);
    }

    private Grid(Cell[,] cells, SudokuDefinition definition)
    {
        _cells = cells;
        Definition = definition;
    }

    public SudokuDefinition Definition { get; }

    public int? GetValue(Position position)
    {
        Definition.EnsurePositionInRange(position);
        return _cells[position.Row, position.Column].Value;
    }

    public bool IsPrefilled(Position position)
    {
        Definition.EnsurePositionInRange(position);
        return _cells[position.Row, position.Column].IsPrefilled;
    }

    public void SetValue(Position position, int value)
    {
        Definition.EnsurePositionInRange(position);
        Definition.EnsureValueInRange(value, nameof(value));
        _cells[position.Row, position.Column].SetValue(value);
    }

    public void Clear(Position position)
    {
        Definition.EnsurePositionInRange(position);
        _cells[position.Row, position.Column].SetValue(null);
    }

    public bool IsComplete()
    {
        for (int row = 0; row < Definition.GridSize; row++)
            for (int column = 0; column < Definition.GridSize; column++)
                if (_cells[row, column].Value is null)
                    return false;
        return true;
    }

    public Grid Clone()
    {
        var copy = new Cell[Definition.GridSize, Definition.GridSize];
        for (int row = 0; row < Definition.GridSize; row++)
            for (int column = 0; column < Definition.GridSize; column++)
                copy[row, column] = new Cell(_cells[row, column].Value, _cells[row, column].IsPrefilled);
        return new Grid(copy, Definition);
    }

    public static Grid FromArrays(int?[,] values, bool[,] prefilledMask, SudokuDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(prefilledMask);
        ArgumentNullException.ThrowIfNull(definition);
        if (values.GetLength(0) != definition.GridSize || values.GetLength(1) != definition.GridSize)
            throw new ArgumentException($"Values array must be {definition.GridSize}x{definition.GridSize}.", nameof(values));
        if (prefilledMask.GetLength(0) != definition.GridSize || prefilledMask.GetLength(1) != definition.GridSize)
            throw new ArgumentException($"Prefilled-cell mask must be {definition.GridSize}x{definition.GridSize}.", nameof(prefilledMask));

        var grid = new Grid(definition);
        for (int row = 0; row < definition.GridSize; row++)
            for (int column = 0; column < definition.GridSize; column++)
            {
                if (values[row, column] is int value)
                    definition.EnsureValueInRange(value, nameof(values));
                grid._cells[row, column] = new Cell(values[row, column], prefilledMask[row, column]);
            }
        return grid;
    }
}
