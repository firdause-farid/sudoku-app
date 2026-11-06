namespace Sudoku.Core.Application.Commands;

/// <summary>
/// Describes why player input could not be parsed into a command.
/// </summary>
public enum CommandParseError
{
    None,
    CommandRequired,
    UnrecognizedCommand,
    PositionRequired,
    InvalidPositionFormat,
    RowOutOfRange,
    ColumnOutOfRange
}
