namespace Sudoku.Core.Domain.Model;

/// <summary>
/// A single Sudoku cell.
/// </summary>
public sealed class Cell
{
    private readonly SudokuDefinition _definition;

    public int? Value { get; private set; }
    public bool IsPrefilled { get; }

    public Cell(int? value, bool isPrefilled, SudokuDefinition definition)
    {
        _definition = definition ?? throw new ArgumentNullException(nameof(definition));
        if (value is int number && !_definition.IsValueInRange(number))
            throw new ArgumentOutOfRangeException(nameof(value), $"Cell value must be between 1 and {_definition.GridSize} or null.");
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

        if (value is int number && !_definition.IsValueInRange(number))
            throw new ArgumentOutOfRangeException(nameof(value), $"Cell value must be between 1 and {_definition.GridSize} or null.");

        Value = value;
    }
}
