using Sudoku.Core.Domain.Randomness;

namespace Sudoku.Console.Adapters;

/// <summary>
/// Provides random values using the .NET <see cref="Random"/> implementation.
/// </summary>
public sealed class SystemRandomSource : IRandomSource
{
    private readonly Random _random = new();

    public int Next(int exclusiveMaximum) => _random.Next(exclusiveMaximum);
}
