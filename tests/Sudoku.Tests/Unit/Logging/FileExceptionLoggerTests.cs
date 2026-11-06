using Sudoku.Console.Logging;

namespace Sudoku.Tests.Unit.Logging;

[Trait("Category", "Unit")]
public sealed class FileExceptionLoggerTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "sudoku-tests-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_directory))
            Directory.Delete(_directory, recursive: true);
    }

    [Fact]
    public void Log_CreatesMissingDirectoryAndWritesContextAndException()
    {
        string path = Path.Combine(_directory, "logs", "exceptions.log");
        var logger = new FileExceptionLogger(path);

        logger.Log(new InvalidOperationException("error"), "Starting a new game");

        string content = File.ReadAllText(path);
        Assert.Contains("Starting a new game", content);
        Assert.Contains("InvalidOperationException", content);
        Assert.Contains("error", content);
    }

    [Fact]
    public void Log_CalledTwice_AppendsBothEntries()
    {
        string path = Path.Combine(_directory, "exceptions.log");
        var logger = new FileExceptionLogger(path);

        logger.Log(new InvalidOperationException("first"), "context one");
        logger.Log(new InvalidOperationException("second"), "context two");

        string content = File.ReadAllText(path);
        Assert.Contains("first", content);
        Assert.Contains("second", content);
    }

    [Fact]
    public void Log_WhenFileCannotBeWritten_DoesNotThrow()
    {
        Directory.CreateDirectory(_directory);
        string blockingFile = Path.Combine(_directory, "not-a-directory");
        File.WriteAllText(blockingFile, string.Empty);
        var logger = new FileExceptionLogger(Path.Combine(blockingFile, "exceptions.log"));

        var exception = Record.Exception(() => logger.Log(new InvalidOperationException("boom"), "context"));

        Assert.Null(exception);
    }

    [Fact]
    public void Log_RejectsMissingArguments()
    {
        var logger = new FileExceptionLogger(Path.Combine(_directory, "exceptions.log"));

        Assert.Throws<ArgumentNullException>(() => logger.Log(null!, "context"));
        Assert.Throws<ArgumentNullException>(() => logger.Log(new InvalidOperationException(), null!));
    }

    [Fact]
    public void DefaultLogFilePath_IsInsideTheLogsFolderOfTheCurrentDirectory()
    {
        var logger = new FileExceptionLogger();

        string expectedTail = Path.Combine("logs", "exceptions.log");
        Assert.EndsWith(expectedTail, logger.LogFilePath);
    }
}
