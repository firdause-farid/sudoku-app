using Sudoku.Core.Application.Ports;

namespace Sudoku.Console.Adapters;

public sealed class ConsoleAdapter : IConsole
{
    private readonly IExceptionLogger _exceptionLogger;

    public ConsoleAdapter(IExceptionLogger exceptionLogger)
    {
        _exceptionLogger = exceptionLogger ?? throw new ArgumentNullException(nameof(exceptionLogger));
    }

    public void WriteLine(string? text = null) => System.Console.WriteLine(text ?? string.Empty);

    public string? ReadLine() => System.Console.ReadLine();

    public ConsoleKeyInfo ReadKey(bool intercept = false)
    {
        if (System.Console.IsInputRedirected)
            return default;

        try
        {
            return System.Console.ReadKey(intercept);
        }
        catch (InvalidOperationException exception)
        {
            // Some hosts have no interactive console even when input isn't reported as redirected.
            _exceptionLogger.Log(exception, "Reading the replay key; continuing without the pause");
            return default;
        }
    }
}
