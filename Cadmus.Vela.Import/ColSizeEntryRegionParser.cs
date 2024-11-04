using Cadmus.Import.Proteus;
using Cadmus.Mat.Bricks;
using Cadmus.Epigraphy.Parts;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Cadmus.Vela.Import;

/// <summary>
/// VeLA column misure (campo), misure_supporto and misure_specchio entry
/// region parser. This targets <see cref="EpiSupportPart.FieldSize"/>,
/// <see cref="EpiSupportPart.SupportSize"/>,
/// <see cref="EpiSupportPart.MirrorSize"/>.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.vela.col-misure")]
public sealed class ColSizeEntryRegionParser(
    ILogger<ColSizeEntryRegionParser>? logger = null) : EntryRegionParser,
    IEntryRegionParser
{
    private const string COL_MISURE_CAMPO = "col-misure";
    private const string COL_MISURE_SUPPORTO = "col-misure_supporto";
    private const string COL_MISURE_SPECCHIO = "col-misure_specchio";
    private readonly ILogger<ColSizeEntryRegionParser>? _logger = logger;

    /// <summary>
    /// Determines whether this parser is applicable to the specified
    /// region. Typically, the applicability is determined via a configurable
    /// nested object, having parameters like region tag(s) and paths.
    /// </summary>
    /// <param name="set">The entries set.</param>
    /// <param name="regions">The regions.</param>
    /// <param name="regionIndex">Index of the region.</param>
    /// <returns>
    ///   <c>true</c> if applicable; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool IsApplicable(EntrySet set, IReadOnlyList<EntryRegion> regions,
        int regionIndex)
    {
        ArgumentNullException.ThrowIfNull(set);
        ArgumentNullException.ThrowIfNull(regions);

        return regions[regionIndex].Tag == COL_MISURE_CAMPO ||
            regions[regionIndex].Tag == COL_MISURE_SPECCHIO ||
            regions[regionIndex].Tag == COL_MISURE_SUPPORTO;
    }

    private PhysicalSize? ParseSize(string value)
    {
        // parse width + height from value like "10x20.5" (cm)
        int i = value.IndexOf('x');
        if (i == -1) return null;

        if (!float.TryParse(value[..i], NumberStyles.Float,
            CultureInfo.InvariantCulture, out float w))
        {
            Logger?.LogError("Invalid width in size: {Value}", value);
            return null;
        }
        if (!float.TryParse(value[(i + 1)..], NumberStyles.Float,
            CultureInfo.InvariantCulture, out float h))
        {
            Logger?.LogError("Invalid height in size: {Value}", value);
            return null;
        }

        return new()
        {
            W = new PhysicalDimension
            {
                Value = w,
                Unit = "cm"
            },
            H = new PhysicalDimension
            {
                Value = h,
                Unit = "cm"
            }
        };
    }

    /// <summary>
    /// Parses the region of entries at <paramref name="regionIndex" />
    /// in the specified <paramref name="regions" />.
    /// </summary>
    /// <param name="set">The entries set.</param>
    /// <param name="regions">The regions.</param>
    /// <param name="regionIndex">Index of the region in the set.</param>
    /// <returns>
    /// The index to the next region to be parsed.
    /// </returns>
    /// <exception cref="ArgumentNullException">set or regions</exception>
    public int Parse(EntrySet set, IReadOnlyList<EntryRegion> regions,
        int regionIndex)
    {
        ArgumentNullException.ThrowIfNull(set);
        ArgumentNullException.ThrowIfNull(regions);

        CadmusEntrySetContext ctx = (CadmusEntrySetContext)set.Context;
        EntryRegion region = regions[regionIndex];

        if (ctx.CurrentItem == null)
        {
            _logger?.LogError("misure column without any item at region {Region}",
                region);
            throw new InvalidOperationException(
                "misure column without any item at region " + region);
        }

        DecodedTextEntry txt = (DecodedTextEntry)
            set.Entries[region.Range.Start.Entry + 1];
        // lowercase because we have sometimes X and sometimes x
        string? value = VelaHelper.FilterValue(txt.Value, true);
        if (!string.IsNullOrEmpty(value))
        {
            PhysicalSize? size = ParseSize(value);
            if (size == null)
            {
                _logger?.LogError("invalid size at region {Region}: \"{Size}\"",
                    region, value);
            }
            else
            {
                EpiSupportPart part =
                    ctx.EnsurePartForCurrentItem<EpiSupportPart>();

                switch (region.Tag)
                {
                    case COL_MISURE_CAMPO:
                        part.HasField = true;
                        part.FieldSize = size;
                        break;
                    case COL_MISURE_SUPPORTO:
                        part.SupportSize = size;
                        break;
                    case COL_MISURE_SPECCHIO:
                        part.HasMirror = true;
                        part.MirrorSize = size;
                        break;
                }
            }
        }

        return regionIndex + 1;
    }
}
