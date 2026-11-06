using Sudoku.Core.Application.Ports;
using Sudoku.Core.Application.Runtime;
using Sudoku.Core.Domain.Game;
using Sudoku.Core.Domain.Generation;
using Sudoku.Core.Domain.Randomness;
using Sudoku.Core.Domain.Validation;
using Sudoku.Core.Presentation.Messages;
using Sudoku.Core.Presentation.Rendering;
using Sudoku.Console.Adapters;
using Sudoku.Console.Logging;

namespace Sudoku.Console;

internal static class Program
{
    private static int Main(string[] args)
    {
        var exceptionLogger = new FileExceptionLogger();
        try
        {
            bool uiEnhancements = args.Contains("--ui-enhancements", StringComparer.OrdinalIgnoreCase);

            IConsole console = new ConsoleAdapter(exceptionLogger);
            IRandomSource random = new SystemRandomSource();
            ISudokuSolutionGenerator solutionGenerator = new SudokuSolutionGenerator();
            ISudokuSolutionCounter solutionCounter = new SudokuSolutionCounter();
            var generator = new PuzzleGenerator(random, solutionGenerator, solutionCounter);
            IValidator validator = new SudokuValidator();
            ISudokuGameFactory gameFactory = new SudokuGameFactory(generator, validator, random);
            IRenderer renderer = uiEnhancements
                ? new GridRenderer(new EnhancedGridStyle())
                : new GridRenderer(new PlainGridStyle());
            var commandHandler = new ConsoleCommandHandler(console, renderer);
            var gameRunner = new ConsoleGameRunner(
                console,
                gameFactory,
                commandHandler,
                exceptionLogger);
            gameRunner.Run();
            return 0;
        }
        catch (Exception exception)
        {
            exceptionLogger.Log(exception, "Unhandled application exception");
            System.Console.Error.WriteLine(UserMessages.UnexpectedError);
            return 1;
        }
    }
}



