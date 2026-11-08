using Sudoku.Console.Configuration;
using Sudoku.Core.Domain.Model;

namespace Sudoku.Tests.Unit.Configuration;

[Trait("Category", "Unit")]
public sealed class SudokuDefinitionCatalogTests : IDisposable
{
    private const string ValidJson = """
        {
          "id": "mini-6x6", "gridSize": 6, "subgridRows": 2, "subgridColumns": 3,
          "symbols": ["1","2","3","4","5","6"], "rowLabels": ["A","B","C","D","E","F"],
          "minimumPrefilledCellCount": 8, "defaultPrefilledCellCount": 18
        }
        """;

    private readonly string _directory =
        Path.Combine(Path.GetTempPath(), "sudoku-catalog-tests-" + Guid.NewGuid().ToString("N"));

    public SudokuDefinitionCatalogTests() => Directory.CreateDirectory(_directory);

    public void Dispose() => Directory.Delete(_directory, recursive: true);

    private void WriteFile(string name, string json) => File.WriteAllText(Path.Combine(_directory, name), json);

    [Fact]
    public void Load_MissingDirectory_ReturnsEmptyCatalog()
    {
        var catalog = SudokuDefinitionCatalog.Load(Path.Combine(_directory, "does-not-exist"));

        Assert.Empty(catalog.Ids);
        Assert.False(catalog.TryGet("mini-6x6", out _));
    }

    [Fact]
    public void Load_ValidFile_ReadsDefinitionAndMatchesIdIgnoringCase()
    {
        WriteFile("any-name.json", ValidJson);

        var catalog = SudokuDefinitionCatalog.Load(_directory);

        Assert.True(catalog.TryGet("MINI-6X6", out SudokuDefinition definition));
        Assert.Equal(6, definition.GridSize);
        Assert.Equal(new[] { "mini-6x6" }, catalog.Ids);
    }

    [Fact]
    public void Load_FileBreakingADefinitionRule_NamesTheFile()
    {
        WriteFile("good.json", ValidJson);
        WriteFile("bad-labels.json", ValidJson.Replace("\"A\",\"B\",\"C\",\"D\",\"E\",\"F\"", "\"A\",\"B\""));

        var exception = Assert.Throws<InvalidOperationException>(() => SudokuDefinitionCatalog.Load(_directory));

        Assert.Contains("bad-labels.json", exception.Message);
        Assert.DoesNotContain("good.json", exception.Message);
    }

    [Fact]
    public void Load_MalformedJson_NamesTheFile()
    {
        WriteFile("broken.json", "{ not json");

        var exception = Assert.Throws<InvalidOperationException>(() => SudokuDefinitionCatalog.Load(_directory));

        Assert.Contains("broken.json", exception.Message);
    }

    [Fact]
    public void Load_TwoFilesWithTheSameId_Throws()
    {
        WriteFile("one.json", ValidJson);
        WriteFile("two.json", ValidJson);

        Assert.Throws<InvalidOperationException>(() => SudokuDefinitionCatalog.Load(_directory));
    }
}
