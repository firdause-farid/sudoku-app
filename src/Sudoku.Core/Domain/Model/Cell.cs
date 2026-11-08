namespace Sudoku.Core.Domain.Model;

/// <summary>
/// A single Sudoku cell. Whether a value is allowed on a particular grid is checked by the grid, not the cell.
/// </summary>
public sealed class Cell
{
    public int? Value { get; private set; }
    public bool IsPrefilled { get; }

    public Cell(int? value, bool isPrefilled)
    {
        if (isPrefilled && value is null)
            throw new ArgumentException("A prefilled cell must have a value.", nameof(value));

        Value = value;
        IsPrefilled = isPrefilled;
    }

    /// <summary>
    /// Sets the value for a cell that was not prefilled.
    /// </summary>
    public void SetValue(int? value)
    {
        if (IsPrefilled)
            throw new InvalidOperationException("Cannot modify a pre-filled cell.");

        Value = value;
    }
}
