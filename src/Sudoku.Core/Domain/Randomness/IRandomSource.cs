namespace Sudoku.Core.Domain.Randomness;

/// <summary>
/// Supplies random values without coupling domain logic to a particular random generator.
/// </summary>
public interface IRandomSource
{
    /// <summary>
    /// Returns a non-negative random integer less than <paramref name="exclusiveMaximum"/>.
    /// </summary>
    int Next(int exclusiveMaximum);
}
