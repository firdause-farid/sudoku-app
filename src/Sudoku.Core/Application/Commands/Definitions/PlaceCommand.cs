using Sudoku.Core.Domain.Model;

namespace Sudoku.Core.Application.Commands;

/// <summary>
/// Put a number in a cell, for example "B3 7".
/// </summary>
public sealed record PlaceCommand(Position Position, int Number) : Command;
