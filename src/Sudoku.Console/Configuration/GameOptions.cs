using Sudoku.Core.Domain.Model;

namespace Sudoku.Console.Configuration;

/// <summary>The validated start-up settings for one run of the game.</summary>
/// <param name="Definition">Grid size, symbols and subgrid shape.</param>
/// <param name="PrefilledCellCount">Prefilled-cell count override, or null to use the definition's default.</param>
/// <param name="UiEnhancements">Whether to draw the grid with the styled (ANSI) look.</param>
internal sealed record GameOptions(SudokuDefinition Definition, int? PrefilledCellCount, bool UiEnhancements);
