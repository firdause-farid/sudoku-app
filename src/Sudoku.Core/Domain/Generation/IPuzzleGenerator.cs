namespace Sudoku.Core.Domain.Generation;

using Sudoku.Core.Domain.Model;

/// <summary>
/// Generates Sudoku puzzles.
/// </summary>
public interface IPuzzleGenerator
{
    (Grid Puzzle, int[,] Solution) Generate(int prefilledCount = PuzzleGenerationSettings.RequiredPrefilledCellCount);

    (Grid Puzzle, int[,] Solution) Generate(int prefilledCount, SudokuDefinition definition) => Generate(prefilledCount);
}
