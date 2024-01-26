using MongoDB.Libmongocrypt;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Cadmus.Vela.Import;

/// <summary>
/// Helper class for VeLA.
/// </summary>
internal static partial class VelaHelper
{
    private static readonly HashSet<string> _emptyValues = [
        "n\\d", "n/d"
    ];

    // thesauri IDs
    public const string T_CATEGORIES_FUNCTIONS = "categories_functions@en";

    [GeneratedRegex(@"\s+")]
    private static partial Regex WsRegex();

    /// <summary>
    /// Filters a cell value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>Filtered value.</returns>
    public static string? FilterValue(string? value)
    {
        if (string.IsNullOrEmpty(value)) return value;

        // trim and lowercase
        value = value.Trim().ToLowerInvariant();

        // normalize whitespaces to single space
        value = WsRegex().Replace(value, " ");

        return _emptyValues.Contains(value) ? null : value;
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
        value = FilterValue(value);
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
        value = FilterValue(value);
        return value switch
        {
            "si" => true,
            "no" => false,
            _ => false
        };
    }
}
