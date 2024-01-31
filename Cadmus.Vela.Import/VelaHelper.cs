using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cadmus.Import.Proteus;
using Proteus.Core.Regions;
using Microsoft.Extensions.Logging;

namespace Cadmus.Vela.Import;

/// <summary>
/// Helper class for VeLA.
/// </summary>
internal static partial class VelaHelper
{
    private static readonly HashSet<string> _emptyValues = [
        "n\\d", "n/d", "N\\D", "N/D"
    ];

    // flag values
    public const int F_IMPORTED = 1;
    public const int F_NOT_INTERPRETABLE = 32;

    // thesauri IDs
    public const string T_SUPPORT_FUNCTIONS = "grf-support-functions@en";
    public const string T_GRF_SUPPORT_MATERIALS = "grf-support-materials@en";
    public const string T_GRF_SUPPORT_OBJECT_TYPES = "grf-support-object-types@en";
    public const string T_GRF_SUPPORT_TYPES = "grf-support-types@en";
    public const string T_GRF_PERIODS = "grf-periods@en";
    public const string T_GRF_TECHNIQUES = "grf-techniques@en";
    public const string T_GRF_TOOLS = "grf-tools@en";
    public const string T_GRF_WRITING_CASING = "grf-writing-casing@en";
    public const string T_GRF_WRITING_GLOTTOLOGS = "grf-writing-glottologs@en";
    public const string T_GRF_WRITING_LANGUAGES = "grf-writing-languages@en";
    public const string T_GRF_WRITING_SCRIPTS = "grf-writing-scripts@en";
    public const string T_GRF_WRITING_SYSTEMS = "grf-writing-systems@en";

    [GeneratedRegex(@"\s+")]
    private static partial Regex WsRegex();

    /// <summary>
    /// Filters a cell value.
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
    /// <returns>List.</returns>
    public static IList<string> GetValueList(string? value, bool lowercase)
    {
        if (string.IsNullOrEmpty(value)) return Array.Empty<string>();

        return (from v in value.Split(" ", StringSplitOptions.RemoveEmptyEntries)
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
    /// Gets the nullable boolean value corresponding to the specified cell
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
    /// Gets the date value with format DD/MM/YYYY.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>Value or null if empty or invalid.</returns>
    public static DateTime? GetDateValue(string? value)
    {
        if (string.IsNullOrEmpty(value)) return null;

        value = FilterValue(value, true);
        // parse from format DD/MM/YYYY
        if (value!.Length == 10 &&
            int.TryParse(value[..2], out int day) &&
            int.TryParse(value.AsSpan(3, 2), out int month) &&
            int.TryParse(value.AsSpan(6, 4), out int year))
        {
            return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
        }

        return null;
    }

    public static string GetThesaurusId(CadmusEntrySetContext ctx,
        EntryRegion region, string thesaurusId, string value,
        ILogger? logger)
    {
        string? id = ctx.ThesaurusEntryMap!.GetEntryId(thesaurusId,
            value.ToLowerInvariant());

        if (id == null)
        {
            logger?.LogError("Unknown value for {tag}: {value} " +
                "at region {region}", region.Tag, value, region);
            id = value;
        }

        return id;
    }
}
