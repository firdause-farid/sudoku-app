namespace Sudoku.Core.Application.Ports;

/// <summary>
/// Records application exceptions for developer diagnostics.
/// </summary>
public interface IExceptionLogger
{
    void Log(Exception exception, string context);
}
