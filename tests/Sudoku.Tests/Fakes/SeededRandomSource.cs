using Sudoku.Core.Domain.Randomness;

namespace Sudoku.Tests.Fakes;

/// <summary>
/// Provides repeatable random values for tests.
/// </summary>
internal sealed class SeededRandomSource : IRandomSource
{
    private readonly Random _random;

    public SeededRandomSource(int seed)
    {
        _random = new Random(seed);
    }

    public int Next(int exclusiveMaximum) => _random.Next(exclusiveMaximum);
}
