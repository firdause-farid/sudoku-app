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
    public static string RowRange => Get(nameof(RowRange));
    public static string ColumnRange => Get(nameof(ColumnRange));
    public static string UnrecognizedCommand => Get(nameof(UnrecognizedCommand));
    public static string InvalidMoveNumber => Get(nameof(InvalidMoveNumber));
    public static string MoveAccepted => Get(nameof(MoveAccepted));
    public static string NoEmptyCellsToHint => Get(nameof(NoEmptyCellsToHint));
    public static string NoRuleViolations => Get(nameof(NoRuleViolations));
    public static string GameCompleted => Get(nameof(GameCompleted));
    public static string ReplayPrompt => Get(nameof(ReplayPrompt));
    public static string ThanksForPlaying => Get(nameof(ThanksForPlaying));
    public static string UnableToStartGame => Get(nameof(UnableToStartGame));
    public static string UnexpectedError => Get(nameof(UnexpectedError));
    public static string GridHeader => Get(nameof(GridHeader));
    public static string GridHeaderSeparator => Get(nameof(GridHeaderSeparator));
    public static string EmptyCell => Get(nameof(EmptyCell));

    public static string InvalidCommand(string details) => Format(nameof(InvalidCommand), details);
    public static string PrefilledCell(string position) => Format(nameof(PrefilledCell), position);
    public static string Hint(string position, int value) => Format(nameof(Hint), position, value);
    public static string DuplicateRow(int value, char row) => Format(nameof(DuplicateRow), value, row);
    public static string DuplicateColumn(int value, int column) => Format(nameof(DuplicateColumn), value, column);
    public static string DuplicateSubgrid(int value) => Format(nameof(DuplicateSubgrid), value);
    private static string Get(string key) =>
        ResourceManager.GetString(key, CultureInfo.CurrentUICulture)
        ?? throw new MissingManifestResourceException($"Missing user message resource '{key}'.");

    private static string Format(string key, params object[] arguments) =>
        string.Format(CultureInfo.CurrentCulture, Get(key), arguments);
}
