using Sudoku.Core.Domain.Model;
using Sudoku.Core.Presentation.Messages;

namespace Sudoku.Core.Application.Commands.Handlers;

internal static class CommandHandlerInput
{
    public static bool TryGetParts(string input, out string[] parts)
    {
        parts = input.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 2;
    }

    public static bool TryGetPosition(string input, out Position position, out string errorMessage)
    {
        if (Position.TryParse(input, out position, out PositionParseError error))
        {
            errorMessage = string.Empty;
            return true;
        }

        errorMessage = error switch
        {
            PositionParseError.Required => UserMessages.PositionRequired,
            PositionParseError.RowOutOfRange => UserMessages.RowRange,
            PositionParseError.ColumnOutOfRange => UserMessages.ColumnRange,
            _ => UserMessages.PositionFormat
        };
        return false;
    }
}
