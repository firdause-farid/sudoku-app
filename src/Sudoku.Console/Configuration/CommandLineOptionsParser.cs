using System.Globalization;
using Sudoku.Core.Domain.Model;
using Sudoku.Core.Presentation.Messages;

namespace Sudoku.Console.Configuration;

/// <summary>
/// Turns command-line arguments into <see cref="GameOptions"/>. Bad input is reported as a message,
/// not an exception. To add a setting: add a property to <see cref="GameOptions"/>, then one read step in <see cref="TryParse"/>.
/// </summary>
internal sealed class CommandLineOptionsParser(Func<SudokuDefinitionCatalog> loadCatalog)
{
    public const string UiEnhancementsOption = "--ui-enhancements";
    public const string VariantOption = "--variant";
    public const string PrefilledCellsOption = "--prefilled-cells";

    private static readonly string[] FlagOptions = [UiEnhancementsOption];
    private static readonly string[] ValueOptions = [VariantOption, PrefilledCellsOption];

    public bool TryParse(string[] args, out GameOptions? options, out string error)
    {
        options = null;

        if (!TryValidateArguments(args, out error) ||
            !TryReadValue(args, VariantOption, out string? variantId, out error) ||
            !TryResolveDefinition(variantId, out SudokuDefinition? definition, out error) ||
            !TryReadValue(args, PrefilledCellsOption, out string? prefilledText, out error) ||
            !TryResolvePrefilledCellCount(prefilledText, definition!, out int? prefilledCellCount, out error))
        {
            return false;
        }

        options = new GameOptions(definition!, prefilledCellCount, ReadFlag(args, UiEnhancementsOption));
        error = string.Empty;
        return true;
    }

    /// <summary>Rejects unknown arguments and options given more than once, so a typo is never silently ignored.</summary>
    private static bool TryValidateArguments(string[] args, out string error)
    {
        error = string.Empty;
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < args.Length; i++)
        {
            string argument = args[i];
            bool takesValue = ValueOptions.Contains(argument, StringComparer.OrdinalIgnoreCase);
            if (!takesValue && !FlagOptions.Contains(argument, StringComparer.OrdinalIgnoreCase))
            {
                error = UserMessages.UnrecognizedArgument(argument, string.Join(", ", FlagOptions.Concat(ValueOptions)));
                return false;
            }

            if (!seen.Add(argument))
            {
                error = UserMessages.DuplicateOption(argument);
                return false;
            }

            // Skip over this option's value. A missing value is reported later by TryReadValue.
            if (takesValue && i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal))
                i++;
        }

        return true;
    }

    private bool TryResolveDefinition(string? variantId, out SudokuDefinition? definition, out string error)
    {
        error = string.Empty;
        if (variantId is null)
        {
            definition = SudokuDefinition.Classic;
            return true;
        }

        // The catalog is only read when a variant is requested, so the default run needs no Variants folder.
        SudokuDefinitionCatalog catalog = loadCatalog();
        if (catalog.TryGet(variantId, out SudokuDefinition found))
        {
            definition = found;
            return true;
        }

        definition = null;
        error = UserMessages.UnknownVariant(variantId, string.Join(", ", catalog.Ids));
        return false;
    }

    private static bool TryResolvePrefilledCellCount(
        string? text, SudokuDefinition definition, out int? count, out string error)
    {
        count = null;
        error = string.Empty;
        if (text is null)
            return true;

        if (!int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out int parsed))
        {
            error = UserMessages.PrefilledCellsNotWholeNumber(PrefilledCellsOption);
            return false;
        }

        if (!definition.IsValidPrefilledCellCount(parsed))
        {
            error = UserMessages.PrefilledCellsOutOfRange(
                PrefilledCellsOption, definition.MinimumPrefilledCellCount, definition.MaximumPrefilledCellCount, definition.Id);
            return false;
        }

        count = parsed;
        return true;
    }

    private static bool ReadFlag(string[] args, string option) =>
        args.Contains(option, StringComparer.OrdinalIgnoreCase);

    /// <summary>Reads "--option value". A missing option gives a null value; an option with no value is an error.</summary>
    private static bool TryReadValue(string[] args, string option, out string? value, out string error)
    {
        value = null;
        error = string.Empty;

        int index = Array.FindIndex(args, argument => argument.Equals(option, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
            return true;

        if (index == args.Length - 1 || args[index + 1].StartsWith("--", StringComparison.Ordinal))
        {
            error = UserMessages.OptionRequiresValue(option);
            return false;
        }

        value = args[index + 1];
        return true;
    }
}
