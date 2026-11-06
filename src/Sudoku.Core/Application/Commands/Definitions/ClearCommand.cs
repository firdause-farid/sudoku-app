using Sudoku.Core.Domain.Model;

namespace Sudoku.Core.Application.Commands;

/// <summary>
/// Empty a cell, for example "C5 clear".
/// </summary>
public sealed record ClearCommand(Position Position) : Command;
