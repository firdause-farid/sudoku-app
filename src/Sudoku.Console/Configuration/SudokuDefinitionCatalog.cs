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
        return new SudokuDefinitionCatalog(ReadDefinitions(directory));
    }

    private static SudokuDefinition[] ReadDefinitions(string directory)
    {
        try
        {
            return Directory.GetFiles(directory, "*.json")
                .Order(StringComparer.OrdinalIgnoreCase)
                .Select(ReadDefinition)
                .ToArray();
        }
        catch (DirectoryNotFoundException)
        {
            // No variants folder just means no variants. (Directory.Exists is not used: it also says "false"
            // when the folder exists but cannot be accessed, which would hide that problem.)
            return [];
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            // An unreadable folder is reported like a bad file: as a message, not a crash.
            throw new InvalidOperationException(
                $"The variants folder '{directory}' could not be read: {exception.Message}", exception);
        }
    }

    public bool TryGet(string id, out SudokuDefinition definition) =>
        _definitions.TryGetValue(id, out definition!);

    public IEnumerable<string> Ids => _definitions.Keys.OrderBy(id => id, StringComparer.OrdinalIgnoreCase);

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Reads one definition file, naming the file in the error so a bad file is easy to find.</summary>
    private static SudokuDefinition ReadDefinition(string path)
    {
        string fileName = Path.GetFileName(path);
        try
        {
            return JsonSerializer.Deserialize<SudokuDefinition>(File.ReadAllText(path), JsonOptions)
                ?? throw new InvalidOperationException($"Sudoku definition file '{fileName}' is empty.");
        }
        catch (Exception exception) when (exception is JsonException or ArgumentException)
        {
            throw new InvalidOperationException(
                $"Sudoku definition file '{fileName}' is not valid: {exception.Message}", exception);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            throw new InvalidOperationException(
                $"Sudoku definition file '{fileName}' could not be read: {exception.Message}", exception);
        }
    }
}
