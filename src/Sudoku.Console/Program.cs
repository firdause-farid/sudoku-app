using Sudoku.Core.Application.Ports;
using Sudoku.Core.Application.Commands;
using Sudoku.Core.Application.Commands.Handlers;
using Sudoku.Core.Application.Runtime;
using Sudoku.Core.Domain.Game;
using Sudoku.Core.Domain.Generation;
using Sudoku.Core.Domain.Randomness;
using Sudoku.Core.Domain.Validation;
using Sudoku.Core.Presentation.Messages;
using Sudoku.Core.Presentation.Rendering;
using Sudoku.Console.Adapters;
using Sudoku.Console.Configuration;
using Sudoku.Console.Logging;
using Sudoku.Core.Domain.Model;

namespace Sudoku.Console;

internal static class Program
{
    private static int Main(string[] args)
    {
        var exceptionLogger = new FileExceptionLogger();
        try
        {
            bool uiEnhancements = args.Contains("--ui-enhancements", StringComparer.OrdinalIgnoreCase);
            string? variantId = ReadOption(args, "--variant");
            SudokuDefinition definition = SudokuDefinition.Classic;
            if (variantId is not null)
            {
                SudokuDefinitionCatalog definitionCatalog = SudokuDefinitionCatalog.Load(
                    Path.Combine(AppContext.BaseDirectory, "Variants"));
                if (!definitionCatalog.TryGet(variantId, out definition!))
                {
                    System.Console.Error.WriteLine($"Unknown Sudoku variant '{variantId}'. Choose one of: {string.Join(", ", definitionCatalog.Ids)}.");
                    return 2;
                }
            }

            int? prefilledCellCount = null;
            string? prefilledCellText = ReadOption(args, "--prefilled-cells");
            if (prefilledCellText is not null)
            {
                if (!int.TryParse(prefilledCellText, out int parsedPrefilledCellCount))
                {
                    System.Console.Error.WriteLine("--prefilled-cells must be a whole number.");
                    return 2;
                }
                if (parsedPrefilledCellCount < definition.MinimumPrefilledCellCount || parsedPrefilledCellCount >= definition.GridSize * definition.GridSize)
                {
                    System.Console.Error.WriteLine($"--prefilled-cells must be between {definition.MinimumPrefilledCellCount} and {definition.GridSize * definition.GridSize - 1} for {definition.Id}.");
                    return 2;
                }
                prefilledCellCount = parsedPrefilledCellCount;
            }

            IConsole console = new ConsoleAdapter(exceptionLogger);
            IRandomSource random = new SystemRandomSource();
            ISudokuSolutionGenerator solutionGenerator = new SudokuSolutionGenerator();
            ISudokuSolutionCounter solutionCounter = new SudokuSolutionCounter();
            var generator = new PuzzleGenerator(random, solutionGenerator, solutionCounter);
            IValidator validator = new SudokuValidator();
            ISudokuGameFactory gameFactory = new SudokuGameFactory(generator, validator, random, definition, prefilledCellCount);
            IRenderer renderer = uiEnhancements
                ? new GridRenderer(new EnhancedGridStyle())
                : new GridRenderer(new PlainGridStyle());
            var commandHandler = new ConsoleCommandHandler(console, renderer, new ICommandHandler[]
            {
                new HintCommandHandler(console),
                new CheckCommandHandler(console),
                new QuitCommandHandler(console),
                new PlaceCommandHandler(console, renderer),
                new ClearCommandHandler(console, renderer)
            });
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

    private static string? ReadOption(string[] args, string option)
    {
        int index = Array.FindIndex(args, argument => argument.Equals(option, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
            return null;
        if (index == args.Length - 1 || args[index + 1].StartsWith("--", StringComparison.Ordinal))
            throw new ArgumentException($"{option} requires a value.");
        return args[index + 1];
    }
}



