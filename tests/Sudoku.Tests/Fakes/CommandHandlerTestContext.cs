using Sudoku.Core.Application.Ports;
using Sudoku.Core.Domain.Game;
using Sudoku.Core.Domain.Model;
using Sudoku.Core.Domain.Validation;

namespace Sudoku.Tests.Fakes;

internal static class CommandHandlerTestContext
{
    public static ISudokuGame CreateGame()
    {
        int[,] solution =
        {
            { 5, 3, 4, 6, 7, 8, 9, 1, 2 },
            { 6, 7, 2, 1, 9, 5, 3, 4, 8 },
            { 1, 9, 8, 3, 4, 2, 5, 6, 7 },
            { 8, 5, 9, 7, 6, 1, 4, 2, 3 },
            { 4, 2, 6, 8, 5, 3, 7, 9, 1 },
            { 7, 1, 3, 9, 2, 4, 8, 5, 6 },
            { 9, 6, 1, 5, 3, 7, 2, 8, 4 },
            { 2, 8, 7, 4, 1, 9, 6, 3, 5 },
            { 3, 4, 5, 2, 8, 6, 1, 7, 9 }
        };
        return new SudokuGame(new Grid(SudokuDefinition.Classic), solution, new SudokuValidator(), new SeededRandomSource(1));
    }

    public static IReadOnlyDictionary<string, string> Route(params (string Name, string Value)[] values) =>
        values.ToDictionary(v => v.Name, v => v.Value, StringComparer.OrdinalIgnoreCase);

    public static IRenderer CreateRenderer() => new StubRenderer();

    private sealed class StubRenderer : IRenderer
    {
        public string Render(IGridView grid) => "rendered grid";
    }
}
