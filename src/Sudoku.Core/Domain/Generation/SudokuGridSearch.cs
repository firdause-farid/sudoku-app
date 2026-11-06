using Sudoku.Core.Domain.Model;

namespace Sudoku.Core.Domain.Generation;

internal static class SudokuGridSearch
{
    public static bool TryFindEmpty(int?[,] grid, out int row, out int column)
    {
        for (int currentRow = 0; currentRow < SudokuRules.GridSize; currentRow++)
            for (int currentColumn = 0; currentColumn < SudokuRules.GridSize; currentColumn++)
                if (grid[currentRow, currentColumn] is null)
                {
                    row = currentRow;
                    column = currentColumn;
                    return true;
                }

        row = -1;
        column = -1;
        return false;
    }
}
