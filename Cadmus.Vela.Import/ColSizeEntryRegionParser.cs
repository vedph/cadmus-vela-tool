using Cadmus.Import.Proteus;
using Cadmus.Mat.Bricks;
using Cadmus.Vela.Parts;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Cadmus.Vela.Import;

/// <summary>
/// VeLA column misure entry region parser. This targets
/// <see cref="GrfFramePart.Size"/>.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.vela.col-misure")]
public sealed class ColSizeEntryRegionParser : EntryRegionParser,
    IEntryRegionParser
{
    private readonly ILogger<ColSizeEntryRegionParser>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColSizeEntryRegionParser"/>
    /// class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public ColSizeEntryRegionParser(
        ILogger<ColSizeEntryRegionParser>? logger = null)
    {
        _logger = logger;
    }

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

        return regions[regionIndex].Tag == "col-misure";
    }

    private static PhysicalSize? ParseSize(string value)
    {
        // parse width + height from value like "10x20.5" (cm)
        int i = value.IndexOf('x');
        if (i == -1) return null;

        return new()
        {
            W = new PhysicalDimension
            {
                Value = float.Parse(value[..i], CultureInfo.InvariantCulture),
                Unit = "cm"
            },
            H = new PhysicalDimension
            {
                Value = float.Parse(value[(i + 1)..], CultureInfo.InvariantCulture),
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
            _logger?.LogError("misure column without any item at region {region}",
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
                _logger?.LogError("invalid size at region {region}: \"{size}\"",
                    region, value);
            }
            else
            {
                GrfFramePart part = ctx.EnsurePartForCurrentItem<GrfFramePart>();
                part.Size = size;
            }
        }

        return regionIndex + 1;
    }
}
