using System.Globalization;
using System.Resources;

namespace Sudoku.Core.Presentation.Messages;

/// <summary>
/// Provides strongly named access to the user-facing text in UserMessages.resx.
/// </summary>
public static class UserMessages
{
    private static readonly ResourceManager ResourceManager =
        new("Sudoku.Core.Presentation.Messages.UserMessages", typeof(UserMessages).Assembly);

    // Resource keys for messages that are only available through the parameterised methods below.
    private const string RowRangeKey = "RowRange";
    private const string ColumnRangeKey = "ColumnRange";
    private const string InvalidMoveNumberKey = "InvalidMoveNumber";

    public static string Welcome => Get(nameof(Welcome));
    public static string PuzzleHeading => Get(nameof(PuzzleHeading));
    public static string CommandPrompt => Get(nameof(CommandPrompt));
    public static string CurrentGridHeader => Get(nameof(CurrentGridHeader));
    public static string PositionRequired => Get(nameof(PositionRequired));
    public static string PositionFormat => Get(nameof(PositionFormat));
    public static string UnrecognizedCommand => Get(nameof(UnrecognizedCommand));
    public static string MoveAccepted => Get(nameof(MoveAccepted));
    public static string NoEmptyCellsToHint => Get(nameof(NoEmptyCellsToHint));
    public static string NoRuleViolations => Get(nameof(NoRuleViolations));
    public static string GameCompleted => Get(nameof(GameCompleted));
    public static string ReplayPrompt => Get(nameof(ReplayPrompt));
    public static string ThanksForPlaying => Get(nameof(ThanksForPlaying));
    public static string UnableToStartGame => Get(nameof(UnableToStartGame));
    public static string UnexpectedError => Get(nameof(UnexpectedError));
    public static string EmptyCell => Get(nameof(EmptyCell));

    public static string InvalidCommand(string details) => Format(nameof(InvalidCommand), details);
    public static string RowRangeFor(string first, string last) => Format(RowRangeKey, first, last);
    public static string ColumnRangeFor(int max) => Format(ColumnRangeKey, max);
    public static string InvalidMoveNumberFor(int max) => Format(InvalidMoveNumberKey, 1, max);
    public static string PrefilledCell(string position) => Format(nameof(PrefilledCell), position);
    public static string Hint(string position, string value) => Format(nameof(Hint), position, value);
    public static string DuplicateRow(string value, string row) => Format(nameof(DuplicateRow), value, row);
    public static string DuplicateColumn(string value, int column) => Format(nameof(DuplicateColumn), value, column);
    public static string DuplicateSubgrid(string value, int subgridRows, int subgridColumns) =>
        Format(nameof(DuplicateSubgrid), value, subgridRows, subgridColumns);
    public static string OptionRequiresValue(string option) => Format(nameof(OptionRequiresValue), option);
    public static string UnrecognizedArgument(string argument, string validOptions) =>
        Format(nameof(UnrecognizedArgument), argument, validOptions);
    public static string DuplicateOption(string option) => Format(nameof(DuplicateOption), option);
    public static string UnknownVariant(string id, string available) => Format(nameof(UnknownVariant), id, available);
    public static string PrefilledCellsNotWholeNumber(string option) => Format(nameof(PrefilledCellsNotWholeNumber), option);
    public static string PrefilledCellsOutOfRange(string option, int minimum, int maximum, string variantId) =>
        Format(nameof(PrefilledCellsOutOfRange), option, minimum, maximum, variantId);

    private static string Get(string key) =>
        ResourceManager.GetString(key, CultureInfo.CurrentUICulture)
        ?? throw new MissingManifestResourceException($"Missing user message resource '{key}'.");

    private static string Format(string key, params object[] arguments) =>
        string.Format(CultureInfo.CurrentCulture, Get(key), arguments);
}
