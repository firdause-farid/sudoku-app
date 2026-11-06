using System.Text.Json;
using Sudoku.Core.Domain.Model;

namespace Sudoku.Console.Configuration;

internal sealed class SudokuDefinitionCatalog
{
    private readonly IReadOnlyDictionary<string, SudokuDefinition> _definitions;

    private SudokuDefinitionCatalog(IEnumerable<SudokuDefinition> definitions)
    {
        var indexed = new Dictionary<string, SudokuDefinition>(StringComparer.OrdinalIgnoreCase);
        foreach (SudokuDefinition definition in definitions)
            if (!indexed.TryAdd(definition.Id, definition))
                throw new InvalidOperationException($"Duplicate Sudoku definition ID '{definition.Id}'.");
        _definitions = indexed;
    }

    public static SudokuDefinitionCatalog Load(string directory)
    {
        if (!Directory.Exists(directory))
            return new SudokuDefinitionCatalog([]);

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        SudokuDefinition[] definitions = Directory.GetFiles(directory, "*.json")
            .Order(StringComparer.OrdinalIgnoreCase)
            .Select(path => JsonSerializer.Deserialize<SudokuDefinition>(File.ReadAllText(path), options)
                ?? throw new InvalidOperationException($"Could not read Sudoku definition file '{Path.GetFileName(path)}'."))
            .ToArray();

        return new SudokuDefinitionCatalog(definitions);
    }

    public bool TryGet(string id, out SudokuDefinition definition) =>
        _definitions.TryGetValue(id, out definition!);

    public IEnumerable<string> Ids => _definitions.Keys.OrderBy(id => id, StringComparer.OrdinalIgnoreCase);
}
