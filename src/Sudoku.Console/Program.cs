using Sudoku.Console.Adapters;
using Sudoku.Console.Configuration;
using Sudoku.Console.Logging;
using Sudoku.Core.Application.Commands;
using Sudoku.Core.Application.Commands.Handlers;
using Sudoku.Core.Application.Ports;
using Sudoku.Core.Application.Runtime;
using Sudoku.Core.Domain.Game;
using Sudoku.Core.Domain.Generation;
using Sudoku.Core.Domain.Randomness;
using Sudoku.Core.Domain.Validation;
using Sudoku.Core.Presentation.Messages;
using Sudoku.Core.Presentation.Rendering;

namespace Sudoku.Console;

internal static class Program
{
    private const int SuccessExitCode = 0;
    private const int InvalidArgumentsExitCode = 2;
    private const int ApplicationFailureExitCode = 1;

    private static int Main(string[] args)
    {
        var exceptionLogger = new FileExceptionLogger();
        try
        {
            var parser = new CommandLineOptionsParser(LoadVariantCatalog);
            if (!parser.TryParse(args, out GameOptions? options, out string error))
            {
                System.Console.Error.WriteLine(error);
                return InvalidArgumentsExitCode;
            }

            return CreateGameRunner(options!, exceptionLogger).Run()
                ? SuccessExitCode
                : ApplicationFailureExitCode;
        }
        catch (Exception exception)
        {
            exceptionLogger.Log(exception, "Unhandled application exception");
            System.Console.Error.WriteLine(UserMessages.UnexpectedError);
            return ApplicationFailureExitCode;
        }
    }

    private static SudokuDefinitionCatalog LoadVariantCatalog() =>
        SudokuDefinitionCatalog.Load(Path.Combine(AppContext.BaseDirectory, "Variants"));

    /// <summary>Composition root: chooses the real implementation of everything the game needs.</summary>
    private static ConsoleGameRunner CreateGameRunner(GameOptions options, IExceptionLogger exceptionLogger)
    {
        IConsole console = new ConsoleAdapter(exceptionLogger);
        IRenderer renderer = CreateRenderer(options);

        return new ConsoleGameRunner(
            console,
            CreateGameFactory(options),
            new CommandDispatcher(console, renderer, CreateCommandHandlers(console, renderer)),
            exceptionLogger);
    }

    private static ISudokuGameFactory CreateGameFactory(GameOptions options)
    {
        IRandomSource random = new SystemRandomSource();
        var generator = new PuzzleGenerator(random, new SudokuSolutionGenerator(), new SudokuSolutionCounter());
        return new SudokuGameFactory(
            generator, new SudokuValidator(), random, options.Definition, options.PrefilledCellCount);
    }

    private static IRenderer CreateRenderer(GameOptions options)
    {
        IGridStyle style = options.UiEnhancements ? new EnhancedGridStyle() : new PlainGridStyle();
        return new GridRenderer(style);
    }

    /// <summary>To add a command, implement ICommandHandler and add it to this list.</summary>
    private static ICommandHandler[] CreateCommandHandlers(IConsole console, IRenderer renderer) =>
    [
        new HintCommandHandler(console),
        new CheckCommandHandler(console),
        new QuitCommandHandler(console),
        new PlaceCommandHandler(console, renderer),
        new ClearCommandHandler(console, renderer)
    ];
}
