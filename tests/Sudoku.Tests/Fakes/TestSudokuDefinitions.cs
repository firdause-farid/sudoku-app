using Sudoku.Core.Domain.Model;

namespace Sudoku.Tests.Fakes;

internal static class TestSudokuDefinitions
{
    public static SudokuDefinition Mini6x6 { get; } = new(
        "mini-test",
        6,
        2,
        3,
        ["1", "2", "3", "4", "5", "6"],
        ["A", "B", "C", "D", "E", "F"],
        8,
        18);
}
