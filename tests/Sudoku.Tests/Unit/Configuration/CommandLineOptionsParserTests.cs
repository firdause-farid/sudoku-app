using Sudoku.Console.Configuration;
using Sudoku.Core.Domain.Model;
using Sudoku.Core.Presentation.Messages;

namespace Sudoku.Tests.Unit.Configuration;

[Trait("Category", "Unit")]
public sealed class CommandLineOptionsParserTests : IDisposable
{
    private const string MiniVariantJson = """
        {
          "id": "mini-6x6", "gridSize": 6, "subgridRows": 2, "subgridColumns": 3,
          "symbols": ["1","2","3","4","5","6"], "rowLabels": ["A","B","C","D","E","F"],
          "minimumPrefilledCellCount": 8, "defaultPrefilledCellCount": 18
        }
        """;

    private readonly string _variantsDirectory =
        Path.Combine(Path.GetTempPath(), "sudoku-parser-tests-" + Guid.NewGuid().ToString("N"));

    private int _catalogLoads;

    public CommandLineOptionsParserTests()
    {
        Directory.CreateDirectory(_variantsDirectory);
        File.WriteAllText(Path.Combine(_variantsDirectory, "mini-6x6.json"), MiniVariantJson);
    }

    public void Dispose() => Directory.Delete(_variantsDirectory, recursive: true);

    private CommandLineOptionsParser CreateParser() =>
        new(() =>
        {
            _catalogLoads++;
            return SudokuDefinitionCatalog.Load(_variantsDirectory);
        });

    [Fact]
    public void TryParse_NoArguments_UsesClassicDefaultsWithoutLoadingCatalog()
    {
        bool parsed = CreateParser().TryParse([], out GameOptions? options, out string error);

        Assert.True(parsed);
        Assert.Equal(string.Empty, error);
        Assert.Same(SudokuDefinition.Classic, options!.Definition);
        Assert.Null(options.PrefilledCellCount);
        Assert.False(options.UiEnhancements);
        Assert.Equal(0, _catalogLoads);
    }

    [Fact]
    public void TryParse_UiEnhancementsFlag_IgnoresCase()
    {
        Assert.True(CreateParser().TryParse(["--UI-Enhancements"], out GameOptions? options, out _));
        Assert.True(options!.UiEnhancements);
    }

    [Fact]
    public void TryParse_KnownVariant_SelectsDefinition()
    {
        Assert.True(CreateParser().TryParse(["--variant", "mini-6x6"], out GameOptions? options, out _));
        Assert.Equal("mini-6x6", options!.Definition.Id);
    }

    [Fact]
    public void TryParse_UnknownVariant_ReportsAvailableIds()
    {
        bool parsed = CreateParser().TryParse(["--variant", "nope"], out GameOptions? options, out string error);

        Assert.False(parsed);
        Assert.Null(options);
        Assert.Equal(UserMessages.UnknownVariant("nope", "mini-6x6"), error);
    }

    [Fact]
    public void TryParse_ValidPrefilledCells_IsAccepted()
    {
        Assert.True(CreateParser().TryParse(["--prefilled-cells", "35"], out GameOptions? options, out _));
        Assert.Equal(35, options!.PrefilledCellCount);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("-5")]
    [InlineData("3.5")]
    public void TryParse_PrefilledCellsNotWholeNumber_ReportsError(string value)
    {
        bool parsed = CreateParser().TryParse(["--prefilled-cells", value], out _, out string error);

        Assert.False(parsed);
        Assert.Equal(UserMessages.PrefilledCellsNotWholeNumber("--prefilled-cells"), error);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("81")]
    public void TryParse_PrefilledCellsOutOfRangeForClassic_ReportsRange(string value)
    {
        int minimum = SudokuDefinition.Classic.MinimumPrefilledCellCount;
        bool parsed = CreateParser().TryParse(["--prefilled-cells", value], out _, out string error);

        Assert.False(parsed);
        Assert.Equal(UserMessages.PrefilledCellsOutOfRange("--prefilled-cells", minimum, 80, "classic-9"), error);
    }

    [Fact]
    public void TryParse_PrefilledCellsAreCheckedAgainstTheChosenVariant()
    {
        // 40 is fine for 9x9 but a 6x6 grid has only 36 cells.
        bool parsed = CreateParser().TryParse(
            ["--variant", "mini-6x6", "--prefilled-cells", "40"], out _, out string error);

        Assert.False(parsed);
        Assert.Equal(UserMessages.PrefilledCellsOutOfRange("--prefilled-cells", 8, 35, "mini-6x6"), error);
    }

    [Theory]
    [InlineData("--variant")]
    [InlineData("--prefilled-cells")]
    public void TryParse_OptionWithoutValue_ReportsError(string option)
    {
        bool parsed = CreateParser().TryParse([option], out _, out string error);

        Assert.False(parsed);
        Assert.Equal(UserMessages.OptionRequiresValue(option), error);
    }

    [Fact]
    public void TryParse_OptionFollowedByAnotherOption_ReportsMissingValue()
    {
        bool parsed = CreateParser().TryParse(["--variant", "--ui-enhancements"], out _, out string error);

        Assert.False(parsed);
        Assert.Equal(UserMessages.OptionRequiresValue("--variant"), error);
    }

    [Theory]
    [InlineData("--prefilled-cell")]
    [InlineData("--ui-enhancement")]
    [InlineData("stray")]
    public void TryParse_UnrecognizedArgument_ReportsItAndTheValidOptions(string argument)
    {
        bool parsed = CreateParser().TryParse([argument, "30"], out GameOptions? options, out string error);

        Assert.False(parsed);
        Assert.Null(options);
        Assert.Equal(
            UserMessages.UnrecognizedArgument(argument, "--ui-enhancements, --variant, --prefilled-cells"), error);
    }

    [Theory]
    [InlineData("--VARIANT", "--variant mini-6x6 --VARIANT mini-6x6")]
    [InlineData("--prefilled-cells", "--prefilled-cells 30 --prefilled-cells 31")]
    [InlineData("--ui-enhancements", "--ui-enhancements --ui-enhancements")]
    public void TryParse_RepeatedOption_ReportsIt(string reportedOption, string arguments)
    {
        bool parsed = CreateParser().TryParse(arguments.Split(' '), out _, out string error);

        Assert.False(parsed);
        Assert.Equal(UserMessages.DuplicateOption(reportedOption), error);
    }

    [Fact]
    public void TryParse_AllOptionsTogether_AreAccepted()
    {
        bool parsed = CreateParser().TryParse(
            ["--ui-enhancements", "--variant", "mini-6x6", "--prefilled-cells", "20"], out GameOptions? options, out _);

        Assert.True(parsed);
        Assert.True(options!.UiEnhancements);
        Assert.Equal("mini-6x6", options.Definition.Id);
        Assert.Equal(20, options.PrefilledCellCount);
    }
}
