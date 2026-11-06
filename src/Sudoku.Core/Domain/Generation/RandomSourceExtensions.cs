namespace Sudoku.Core.Domain.Generation;

using Sudoku.Core.Domain.Randomness;

internal static class RandomSourceExtensions
{
    public static void Shuffle<T>(this IRandomSource random, IList<T> items)
    {
        ArgumentNullException.ThrowIfNull(random);
        ArgumentNullException.ThrowIfNull(items);

        for (int index = items.Count - 1; index > 0; index--)
        {
            int swapIndex = random.Next(index + 1);
            (items[index], items[swapIndex]) = (items[swapIndex], items[index]);
        }
    }
}
