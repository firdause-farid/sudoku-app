using Sudoku.Core.Application.Ports;

namespace Sudoku.Tests.Fakes;

public sealed class NullExceptionLogger : IExceptionLogger
{
    public void Log(Exception exception, string context)
    {
    }
}
