using Sudoku.Core.Domain.Model;

namespace Sudoku.Core.Application.Ports;

/// <summary>
/// Renders a Sudoku grid.
/// </summary>
public interface IRenderer
{
    string Render(IGridView grid);
}
