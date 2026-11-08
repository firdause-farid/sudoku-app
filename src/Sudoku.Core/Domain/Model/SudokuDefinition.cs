using System.Globalization;

namespace Sudoku.Core.Domain.Model;

/// <summary>
/// Describes the shape of a Sudoku grid (size, subgrids, symbols and row labels)
/// and how its values and positions are written and read.
/// </summary>
public sealed class SudokuDefinition
{
    public static SudokuDefinition Classic { get; } = new(
        "classic-9",
        9,
        3,
        3,
        ["1", "2", "3", "4", "5", "6", "7", "8", "9"],
        ["A", "B", "C", "D", "E", "F", "G", "H", "I"],
        17,
        30);

    public SudokuDefinition(
        string id,
        int gridSize,
        int subgridRows,
        int subgridColumns,
        IReadOnlyList<string> symbols,
        IReadOnlyList<string> rowLabels,
        int minimumPrefilledCellCount,
        int defaultPrefilledCellCount)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("A definition ID is required.", nameof(id));
        if (gridSize < 2)
            throw new ArgumentOutOfRangeException(nameof(gridSize), "Grid size must be at least 2.");
        if (subgridRows <= 0 || subgridColumns <= 0 || subgridRows * subgridColumns != gridSize)
            throw new ArgumentException("Subgrid row count multiplied by subgrid column count must equal grid size.");
        ArgumentNullException.ThrowIfNull(symbols);
        ArgumentNullException.ThrowIfNull(rowLabels);
        if (symbols.Count != gridSize || symbols.Any(string.IsNullOrWhiteSpace) ||
            symbols.Distinct(StringComparer.OrdinalIgnoreCase).Count() != gridSize)
            throw new ArgumentException("A definition must provide one unique, non-empty symbol per value.", nameof(symbols));
        if (rowLabels.Count != gridSize || rowLabels.Any(string.IsNullOrWhiteSpace) ||
            rowLabels.Distinct(StringComparer.OrdinalIgnoreCase).Count() != gridSize)
            throw new ArgumentException("A definition must provide one unique, non-empty row label per row.", nameof(rowLabels));
        EnsureTypableLabels(rowLabels);
        EnsureTypableSymbols(symbols);
        if (minimumPrefilledCellCount is < 1 || minimumPrefilledCellCount >= gridSize * gridSize)
            throw new ArgumentOutOfRangeException(nameof(minimumPrefilledCellCount));

        Id = id;
        GridSize = gridSize;
        SubgridRows = subgridRows;
        SubgridColumns = subgridColumns;
        Symbols = Array.AsReadOnly(symbols.ToArray());
        RowLabels = Array.AsReadOnly(rowLabels.ToArray());
        MinimumPrefilledCellCount = minimumPrefilledCellCount;
        DefaultPrefilledCellCount = defaultPrefilledCellCount;
        EnsureValidPrefilledCellCount(defaultPrefilledCellCount, nameof(defaultPrefilledCellCount));
    }

    /// <summary>
    /// Text the game already uses: "clear" is the command word in "A1 clear", and "_" is how an empty cell is drawn.
    /// A symbol with either text could never be told apart from them.
    /// </summary>
    private const string ClearCommandWord = "clear";
    private const string EmptyCellMarker = "_";

    private static void EnsureTypableLabels(IReadOnlyList<string> rowLabels)
    {
        foreach (string label in rowLabels)
            if (label.Any(character => char.IsDigit(character) || char.IsWhiteSpace(character)))
                throw new ArgumentException(
                    $"Row label '{label}' must not contain digits or spaces, because a cell is typed as the row label followed by the column number.",
                    nameof(rowLabels));
    }

    private static void EnsureTypableSymbols(IReadOnlyList<string> symbols)
    {
        foreach (string symbol in symbols)
        {
            if (symbol.Any(char.IsWhiteSpace))
                throw new ArgumentException($"Symbol '{symbol}' must not contain spaces, because input is split on spaces.", nameof(symbols));
            if (symbol.Equals(ClearCommandWord, StringComparison.OrdinalIgnoreCase) || symbol == EmptyCellMarker)
                throw new ArgumentException($"Symbol '{symbol}' is reserved by the game and cannot be used as a value.", nameof(symbols));
        }
    }

    public string Id { get; }
    public int GridSize { get; }
    public int SubgridRows { get; }
    public int SubgridColumns { get; }
    public IReadOnlyList<string> Symbols { get; }
    public IReadOnlyList<string> RowLabels { get; }
    public int MinimumPrefilledCellCount { get; }
    public int DefaultPrefilledCellCount { get; }

    /// <summary>Total number of cells in the grid.</summary>
    public int CellCount => GridSize * GridSize;

    /// <summary>The most prefilled cells a puzzle may have: every cell except one, so there is something to solve.</summary>
    public int MaximumPrefilledCellCount => CellCount - 1;

    /// <summary>The single rule for how many prefilled cells a puzzle of this definition may have.</summary>
    public bool IsValidPrefilledCellCount(int count) =>
        count >= MinimumPrefilledCellCount && count <= MaximumPrefilledCellCount;

    public void EnsureValidPrefilledCellCount(int count, string parameterName)
    {
        if (!IsValidPrefilledCellCount(count))
            throw new ArgumentOutOfRangeException(
                parameterName,
                count,
                $"Prefilled cells for '{Id}' must be between {MinimumPrefilledCellCount} and {MaximumPrefilledCellCount}.");
    }

    public bool IsValueInRange(int value) => value >= 1 && value <= GridSize;

    public string FormatValue(int value) => IsValueInRange(value)
        ? Symbols[value - 1]
        : throw new ArgumentOutOfRangeException(nameof(value));

    public bool TryParseValue(string text, out int value)
    {
        int index = -1;
        for (int i = 0; i < Symbols.Count; i++)
            if (Symbols[i].Equals(text, StringComparison.OrdinalIgnoreCase))
            {
                index = i;
                break;
            }

        value = index + 1;
        return index >= 0;
    }

    public string FormatPosition(Position position)
    {
        EnsurePositionInRange(position);
        return $"{RowLabels[position.Row]}{position.Column + 1}";
    }

    public bool TryParsePosition(string? text, out Position position, out PositionParseError error)
    {
        position = default;
        if (string.IsNullOrWhiteSpace(text))
        {
            error = PositionParseError.Required;
            return false;
        }

        string normalized = text.Trim();
        if (!TryMatchRowLabel(normalized, out int row, out string rowLabel))
        {
            error = normalized.Any(char.IsAsciiDigit)
                ? PositionParseError.RowOutOfRange
                : PositionParseError.InvalidFormat;
            return false;
        }

        if (!int.TryParse(normalized[rowLabel.Length..], NumberStyles.None, CultureInfo.InvariantCulture, out int columnNumber))
        {
            error = PositionParseError.InvalidFormat;
            return false;
        }

        if (columnNumber < 1 || columnNumber > GridSize)
        {
            error = PositionParseError.ColumnOutOfRange;
            return false;
        }

        position = new Position(row, columnNumber - 1);
        error = PositionParseError.None;
        return true;
    }

    /// <summary>Finds the row label the text starts with. The longest match wins, so "AB3" is not read as row "A".</summary>
    private bool TryMatchRowLabel(string text, out int row, out string label)
    {
        row = -1;
        label = string.Empty;
        for (int index = 0; index < RowLabels.Count; index++)
        {
            string candidate = RowLabels[index];
            if (candidate.Length > label.Length && text.StartsWith(candidate, StringComparison.OrdinalIgnoreCase))
            {
                row = index;
                label = candidate;
            }
        }

        return row >= 0;
    }

    public void EnsurePositionInRange(Position position)
    {
        if (position.Row >= GridSize || position.Column >= GridSize)
            throw new ArgumentOutOfRangeException(nameof(position), "Position is outside this Sudoku grid.");
    }

    public void EnsureValueInRange(int value, string parameterName)
    {
        if (!IsValueInRange(value))
            throw new ArgumentOutOfRangeException(parameterName, value, $"Value must be between 1 and {GridSize}.");
    }
}
