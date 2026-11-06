using Sudoku.Core.Domain.Model;

namespace Sudoku.Core.Application.Commands;

/// <summary>
/// Parses command-line input into structured commands.
/// </summary>
public static class CommandParser
{
    private const int ExpectedCommandPartCount = 2;

    public static bool TryParse(string? input, out Command command, out CommandParseError error)
    {
        command = null!;
        error = CommandParseError.None;

        if (string.IsNullOrWhiteSpace(input))
        {
            error = CommandParseError.CommandRequired;
            return false;
        }

        string trimmed = input.Trim();
        string normalizedInput = trimmed.ToLowerInvariant();

        if (normalizedInput == "hint")
        {
            command = new HintCommand();
            return true;
        }
        if (normalizedInput == "check")
        {
            command = new CheckCommand();
            return true;
        }
        if (normalizedInput is "quit")
        {
            command = new QuitCommand();
            return true;
        }

        string[] parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != ExpectedCommandPartCount)
        {
            error = CommandParseError.UnrecognizedCommand;
            return false;
        }

        if (parts[1].Equals("clear", StringComparison.OrdinalIgnoreCase))
        {
            if (!TryParsePosition(parts[0], out Position clearPosition, out error))
                return false;

            command = new ClearCommand(clearPosition);
            return true;
        }

        if (!int.TryParse(parts[1], out int number))
        {
            error = CommandParseError.UnrecognizedCommand;
            return false;
        }
        if (!TryParsePosition(parts[0], out Position placePosition, out error))
            return false;

        command = new PlaceCommand(placePosition, number);
        return true;
    }

    private static bool TryParsePosition(string input, out Position position, out CommandParseError error)
    {
        if (Position.TryParse(input, out position, out PositionParseError positionError))
        {
            error = CommandParseError.None;
            return true;
        }

        error = positionError switch
        {
            PositionParseError.Required => CommandParseError.PositionRequired,
            PositionParseError.RowOutOfRange => CommandParseError.RowOutOfRange,
            PositionParseError.ColumnOutOfRange => CommandParseError.ColumnOutOfRange,
            _ => CommandParseError.InvalidPositionFormat
        };
        return false;
    }
}
