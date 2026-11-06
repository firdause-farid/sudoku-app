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

    public static string Welcome => Get(nameof(Welcome));
    public static string PuzzleHeading => Get(nameof(PuzzleHeading));
    public static string CommandPrompt => Get(nameof(CommandPrompt));
    public static string CurrentGridHeader => Get(nameof(CurrentGridHeader));
    public static string CommandRequired => Get(nameof(CommandRequired));
    public static string PositionRequired => Get(nameof(PositionRequired));
    public static string PositionFormat => Get(nameof(PositionFormat));
    public static string RowRange => RowRangeFor("A", "I");
    public static string ColumnRange => ColumnRangeFor(9);
    public static string UnrecognizedCommand => Get(nameof(UnrecognizedCommand));
    public static string InvalidMoveNumber => InvalidMoveNumberFor(9);
    public static string MoveAccepted => Get(nameof(MoveAccepted));
    public static string NoEmptyCellsToHint => Get(nameof(NoEmptyCellsToHint));
    public static string NoRuleViolations => Get(nameof(NoRuleViolations));
    public static string GameCompleted => Get(nameof(GameCompleted));
    public static string ReplayPrompt => Get(nameof(ReplayPrompt));
    public static string ThanksForPlaying => Get(nameof(ThanksForPlaying));
    public static string UnableToStartGame => Get(nameof(UnableToStartGame));
    public static string UnexpectedError => Get(nameof(UnexpectedError));
    public static string GridHeader => Get(nameof(GridHeader));
    public static string EmptyCell => Get(nameof(EmptyCell));

    public static string InvalidCommand(string details) => Format(nameof(InvalidCommand), details);
    public static string RowRangeFor(string first, string last) => Format(nameof(RowRange), first, last);
    public static string ColumnRangeFor(int max) => Format(nameof(ColumnRange), max);
    public static string InvalidMoveNumberFor(int max) => Format(nameof(InvalidMoveNumber), 1, max);
    public static string PrefilledCell(string position) => Format(nameof(PrefilledCell), position);
    public static string Hint(string position, int value) => Format(nameof(Hint), position, value);
    public static string Hint(string position, string value) => Format(nameof(Hint), position, value);
    public static string DuplicateRow(int value, char row) => Format(nameof(DuplicateRow), value, row);
    public static string DuplicateColumn(int value, int column) => Format(nameof(DuplicateColumn), value, column);
    public static string DuplicateSubgrid(int value) => Format(nameof(DuplicateSubgrid), value);
    public static string DuplicateRow(string value, string row) => Format(nameof(DuplicateRow), value, row);
    public static string DuplicateColumn(string value, int column) => Format(nameof(DuplicateColumn), value, column);
    public static string DuplicateSubgrid(string value) => Format(nameof(DuplicateSubgrid), value);
    public static string DuplicateSubgrid(string value, int subgridRows, int subgridColumns) =>
        subgridRows == 3 && subgridColumns == 3
            ? Format(nameof(DuplicateSubgrid), value)
            : Format("DuplicateSubgridOther", value, subgridRows, subgridColumns);
    private static string Get(string key) =>
        ResourceManager.GetString(key, CultureInfo.CurrentUICulture)
        ?? throw new MissingManifestResourceException($"Missing user message resource '{key}'.");

    private static string Format(string key, params object[] arguments) =>
        string.Format(CultureInfo.CurrentCulture, Get(key), arguments);
}
