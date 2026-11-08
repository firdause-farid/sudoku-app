namespace Sudoku.Core.Domain.Model;

internal static class GridViewExtensions
{
    /// <summary>Copies the grid's values into a plain array, which the rule checks and the solver work on.</summary>
    public static int?[,] ToValueArray(this IGridView grid)
    {
        ArgumentNullException.ThrowIfNull(grid);

        int size = grid.Definition.GridSize;
        var values = new int?[size, size];
        for (int row = 0; row < size; row++)
            for (int column = 0; column < size; column++)
                values[row, column] = grid.GetValue(new Position(row, column));

        return values;
    }
}
