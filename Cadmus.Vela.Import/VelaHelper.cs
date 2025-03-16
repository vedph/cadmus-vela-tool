using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cadmus.Import.Proteus;
using Proteus.Core.Regions;
using Microsoft.Extensions.Logging;

namespace Cadmus.Vela.Import;

/// <summary>
/// Helper class for VeLA importer.
/// </summary>
internal static partial class VelaHelper
{
    private static readonly HashSet<string> _emptyValues = [
        "n\\d", "n/d", "N\\D", "N/D"
    ];

    // flag values
    public const int F_IMPORTED = 2;
    public const int F_NOT_INTERPRETABLE = 32;

    // thesauri IDs
    public const string T_CATEGORIES_FN = "categories_fn@en";
    public const string T_CATEGORIES_CNT = "categories_cnt@en";
    public const string T_CATEGORIES_FIG = "categories_fig@en";
    public const string T_DISTRICT_NAME_PIECE_TYPES = "district-name-piece-types@en";
    public const string T_EPI_SUPPORT_FUNCTIONS = "epi-support-functions@en";
    public const string T_EPI_SUPPORT_MATERIALS = "epi-support-materials@en";
    public const string T_EPI_SUPPORT_OBJECT_TYPES = "epi-support-object-types@en";
    public const string T_EPI_SUPPORT_TYPES = "epi-support-types@en";
    public const string T_EPI_TECHNIQUE_TYPES = "epi-technique-types@en";
    public const string T_EPI_TECHNIQUE_TOOLS = "epi-technique-tools@en";
    public const string T_EPI_WRITING_CASINGS = "epi-writing-casings@en";
    public const string T_EPI_WRITING_FEATURES = "epi-writing-features@en";
    public const string T_EPI_WRITING_SCRIPTS = "epi-writing-scripts@en";
    public const string T_PHYSICAL_STATES = "physical-states@en";

    [GeneratedRegex(@"\s+")]
    private static partial Regex WsRegex();

    /// <summary>
    /// Filters a cell value. This trims it, normalizes whitespaces to a single
    /// space, and optionally lowercases it. If the resulting value represents
    /// an empty value, null is returned.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="lowercase">True to lowercase the result.</param>
    /// <returns>Filtered value.</returns>
    public static string? FilterValue(string? value, bool lowercase)
    {
        if (string.IsNullOrEmpty(value)) return value;

        // trim
        value = value.Trim();

        // normalize whitespaces to single space
        value = WsRegex().Replace(value, " ");

        // lowercase if required
        if (lowercase) value = value.ToLowerInvariant();

        return _emptyValues.Contains(value) ? null : value;
    }

    /// <summary>
    /// Gets a list of comma-separated values from the specified text value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="lowercase">True to lowercase the result.</param>
    /// <param name="separators">The optional separators to use. If not specified,
    /// comma will be the default.</param>
    /// <returns>List.</returns>
    public static IList<string> GetValueList(string? value, bool lowercase,
        char[]? separators = null)
    {
        if (string.IsNullOrEmpty(value)) return Array.Empty<string>();

        separators ??= [','];
        return (from v in value.Split(separators,
                    StringSplitOptions.RemoveEmptyEntries)
                select FilterValue(v, lowercase) into v
                where !string.IsNullOrEmpty(v)
                select v).ToList();
    }

    /// <summary>
    /// Gets the nullable boolean value corresponding to the specified cell
    /// string value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>Nullable boolean.</returns>
    public static bool? GetNullableBooleanValue(string? value)
    {
        if (string.IsNullOrEmpty(value)) return null;
        value = FilterValue(value, true);
        return value switch
        {
            "si" => true,
            "no" => false,
            _ => null
        };
    }

    /// <summary>
    /// Gets the boolean value corresponding to the specified cell
    /// string value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>Boolean.</returns>
    public static bool GetBooleanValue(string? value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        value = FilterValue(value, true);
        return value switch
        {
            "si" => true,
            "no" => false,
            _ => false
        };
    }

    /// <summary>
    /// Gets the int value corresponding to the specified cell string value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>Integer.</returns>
    public static int GetIntValue(string? value)
    {
        if (string.IsNullOrEmpty(value)) return 0;
        value = FilterValue(value, true);
        return int.TryParse(value, out int n) ? n : 0;
    }

    /// <summary>
    /// Gets the date value with format DD/MM/YYYY.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>Value or null if empty or invalid.</returns>
    public static DateOnly? GetDateValue(string? value)
    {
        if (string.IsNullOrEmpty(value)) return null;

        value = FilterValue(value, true);
        if (string.IsNullOrEmpty(value)) return null;

        // parse from formats like D/MM/YYYY, DD/M/YYYY, etc.
        // and also tolerate \ for /
        string[] dateParts = value.Replace('\\', '/').Split('/');
        if (dateParts.Length == 3 &&
            int.TryParse(dateParts[0], out int day) &&
            int.TryParse(dateParts[1], out int month) &&
            int.TryParse(dateParts[2], out int year))
        {
            return new DateOnly(year, month, day);
        }

        return null;
    }

    /// <summary>
    /// Gets the thesaurus identifier for the specified entry value in the
    /// specified thesaurus. In case the ID is not found, an error is logged
    /// if <paramref name="logger"/> is not null, and the value itself is
    /// returned.
    /// </summary>
    /// <param name="context">The data context.</param>
    /// <param name="region">The current region.</param>
    /// <param name="thesaurusId">The thesaurus identifier.</param>
    /// <param name="value">The entry value.</param>
    /// <param name="logger">The optional logger.</param>
    /// <returns>ID or value if not found.</returns>
    public static string GetThesaurusId(CadmusEntrySetContext context,
        EntryRegion region, string thesaurusId, string value,
        ILogger? logger)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(region);
        ArgumentNullException.ThrowIfNull(thesaurusId);
        ArgumentNullException.ThrowIfNull(value);

        string? id = context.ThesaurusEntryMap!.GetEntryId(thesaurusId, value);

        if (id == null)
        {
            logger?.LogError("Unknown value for {Tag}: \"{Value}\" " +
                "at region {Region}", region.Tag, value, region);
            id = value;
        }

        return id;
    }
}
