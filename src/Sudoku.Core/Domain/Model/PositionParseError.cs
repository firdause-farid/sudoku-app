namespace Sudoku.Core.Domain.Model;

/// <summary>
/// Describes why text could not be parsed as a <see cref="Position"/>.
/// </summary>
public enum PositionParseError
{
    None,
    Required,
    InvalidFormat,
    RowOutOfRange,
    ColumnOutOfRange
}
