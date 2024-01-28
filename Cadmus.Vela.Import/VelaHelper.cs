using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Cadmus.Vela.Import;

/// <summary>
/// Helper class for VeLA.
/// </summary>
internal static partial class VelaHelper
{
    private static readonly HashSet<string> _emptyValues = [
        "n\\d", "n/d", "N\\D", "N/D"
    ];

    // thesauri IDs
    public const string T_CATEGORIES_FUNCTIONS = "categories_functions@en";
    public const string T_GRF_SUPPORT_OBJECT_TYPES = "grf-support-object-types@en";
    public const string T_GRF_SUPPORT_MATERIALS = "grf-support-materials@en";
    public const string T_GRF_PERIODS = "grf-periods@en";

    [GeneratedRegex(@"\s+")]
    private static partial Regex WsRegex();

    /// <summary>
    /// Filters a cell value.
    /// </summary>
    /// <param name="value">The value.</param>
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
}
