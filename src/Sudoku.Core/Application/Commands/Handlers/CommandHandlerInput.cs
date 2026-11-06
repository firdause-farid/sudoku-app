using Sudoku.Core.Domain.Model;
using Sudoku.Core.Presentation.Messages;

namespace Sudoku.Core.Application.Commands.Handlers;

internal static class CommandHandlerInput
{
    public static bool TryGetParts(string input, int expectedPartCount, out string[] parts)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(expectedPartCount);
        parts = input.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == expectedPartCount;
    }

    public static bool TryGetPosition(string input, SudokuDefinition definition, out Position position, out string errorMessage)
    {
        if (Position.TryParse(input, definition, out position, out PositionParseError error))
        {
            errorMessage = string.Empty;
            return true;
        }

        errorMessage = error switch
        {
            PositionParseError.Required => UserMessages.PositionRequired,
            PositionParseError.RowOutOfRange => UserMessages.RowRangeFor(
                definition.RowLabels[0], definition.RowLabels[^1]),
            PositionParseError.ColumnOutOfRange => UserMessages.ColumnRangeFor(definition.GridSize),
            _ => UserMessages.PositionFormat
        };
        return false;
    }
}
