namespace Sudoku.Core.Domain.Model;

/// <summary>
/// A 9x9 Sudoku grid.
/// </summary>
public sealed class Grid : IGridView
{
    private readonly Cell[,] _cells;

    public Grid()
    {
        _cells = new Cell[SudokuRules.GridSize, SudokuRules.GridSize];
        for (int r = 0; r < SudokuRules.GridSize; r++)
            for (int c = 0; c < SudokuRules.GridSize; c++)
                _cells[r, c] = new Cell(null, false);
    }

    private Grid(Cell[,] cells)
    {
        _cells = cells;
    }

    public int? GetValue(Position position) => _cells[position.Row, position.Column].Value;

    public bool IsPrefilled(Position position) => _cells[position.Row, position.Column].IsPrefilled;

    public void SetValue(Position position, int value) => _cells[position.Row, position.Column].SetValue(value);

    public void Clear(Position position) => _cells[position.Row, position.Column].SetValue(null);

    public bool IsComplete()
    {
        for (int r = 0; r < SudokuRules.GridSize; r++)
            for (int c = 0; c < SudokuRules.GridSize; c++)
                if (_cells[r, c].Value is null)
                    return false;
        return true;
    }

    public Grid Clone()
    {
        var copy = new Cell[SudokuRules.GridSize, SudokuRules.GridSize];
        for (int r = 0; r < SudokuRules.GridSize; r++)
            for (int c = 0; c < SudokuRules.GridSize; c++)
                copy[r, c] = new Cell(_cells[r, c].Value, _cells[r, c].IsPrefilled);
        return new Grid(copy);
    }

    /// <summary>
    /// Creates a grid from a value array and a prefilled-cell mask.
    /// </summary>
    public static Grid FromArrays(int?[,] values, bool[,] prefilledMask)
    {
        if (values.GetLength(0) != SudokuRules.GridSize || values.GetLength(1) != SudokuRules.GridSize)
            throw new ArgumentException($"Values array must be {SudokuRules.GridSize}x{SudokuRules.GridSize}.");
        if (prefilledMask.GetLength(0) != SudokuRules.GridSize || prefilledMask.GetLength(1) != SudokuRules.GridSize)
            throw new ArgumentException($"Prefilled-cell mask must be {SudokuRules.GridSize}x{SudokuRules.GridSize}.");

        var grid = new Grid();
        for (int r = 0; r < SudokuRules.GridSize; r++)
            for (int c = 0; c < SudokuRules.GridSize; c++)
            {
                // Rebuild cells to honour the prefilled-cell mask.
                var cell = new Cell(values[r, c], prefilledMask[r, c]);
                grid._cells[r, c] = cell;
            }
        return grid;
    }

    /// <summary>
    /// Returns a copy of the underlying values.
    /// </summary>
    public int?[,] ToValueArray()
    {
        var result = new int?[SudokuRules.GridSize, SudokuRules.GridSize];
        for (int r = 0; r < SudokuRules.GridSize; r++)
            for (int c = 0; c < SudokuRules.GridSize; c++)
                result[r, c] = _cells[r, c].Value;
        return result;
    }
}
