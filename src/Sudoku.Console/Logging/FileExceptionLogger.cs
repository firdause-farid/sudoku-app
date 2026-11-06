using System.Globalization;
using System.Text;
using Sudoku.Core.Application.Ports;

namespace Sudoku.Console.Logging;

/// <summary>
/// Appends exception details to the current directory's logs folder by default.
/// </summary>
public sealed class FileExceptionLogger : IExceptionLogger
{
    private readonly object _writeLock = new();

    public FileExceptionLogger(string? logFilePath = null)
    {
        LogFilePath = string.IsNullOrWhiteSpace(logFilePath)
            ? Path.Combine(Directory.GetCurrentDirectory(), "logs", "exceptions.log")
            : logFilePath;
    }

    public string LogFilePath { get; }

    public void Log(Exception exception, string context)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(context);

        try
        {
            string? directory = Path.GetDirectoryName(LogFilePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var entry = new StringBuilder()
                .Append('[')
                .Append(DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture))
                .Append("] ")
                .AppendLine(context)
                .AppendLine(exception.ToString())
                .AppendLine()
                .ToString();

            lock (_writeLock)
                File.AppendAllText(LogFilePath, entry, Encoding.UTF8);
        }
        catch (Exception loggingException)
        {
            // Logging must never turn a handled application error into another crash.
            try
            {
                System.Console.Error.WriteLine($"Unable to write the exception log: {loggingException.Message}");
            }
            catch
            {
                // There may be no usable console either.
            }
        }
    }
}
