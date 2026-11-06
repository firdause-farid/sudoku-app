namespace Sudoku.Core.Application.Ports;

/// <summary>
/// Abstraction over console I/O to keep game logic testable.
/// </summary>
public interface IConsole
{
    void WriteLine(string? text = null);
    string? ReadLine();
    ConsoleKeyInfo ReadKey(bool intercept = false);
}
