namespace Sudoku.Core.Domain.Model;

/// <summary>
/// A single Sudoku cell. This is an implementation detail of <see cref="Grid"/>, which is the only code that creates cells
/// and the one that checks a value is allowed on its grid. Everything else sees cells through <see cref="IGridView"/>.
/// </summary>
internal sealed class Cell
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
