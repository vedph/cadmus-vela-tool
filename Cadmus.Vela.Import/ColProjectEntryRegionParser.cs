using Cadmus.Import.Proteus;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;

namespace Cadmus.Vela.Import;

/// <summary>
/// VeLA column segmento_progetto entry region parser. This targets item's flags.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.vela.col-segmento_progetto")]
public sealed class ColProjectEntryRegionParser : EntryRegionParser,
    IEntryRegionParser
{
    private const string COL_SEGMENTO_PROGETTO = "col-segmento_progetto";
    private readonly ILogger<ColProjectEntryRegionParser>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColProjectEntryRegionParser"/>
    /// class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public ColProjectEntryRegionParser(
        ILogger<ColProjectEntryRegionParser>? logger = null)
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

        return regions[regionIndex].Tag == COL_SEGMENTO_PROGETTO;
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
            _logger?.LogError(
                "segmento_progetto column without any item at region {Region}",
                region);
            throw new InvalidOperationException(
                "segmento_progetto column without any item at region " + region);
        }

        DecodedTextEntry txt = (DecodedTextEntry)
            set.Entries[region.Range.Start.Entry + 1];
        string? value = VelaHelper.FilterValue(txt.Value, true);

        switch (value)
        {
            case "vela urbana":
                ctx.CurrentItem.Flags |= 64;
                break;
            case "vela monastica":
                ctx.CurrentItem.Flags |= 128;
                break;
            case "vela palazzo ducale":
                ctx.CurrentItem.Flags |= 256;
                break;
            case "imai":
                ctx.CurrentItem.Flags |= 512;
                break;
            default:
                _logger?.LogError(
                    "Invalid segmento_progetto value at region {Region}: {Value}",
                    region, value);
                break;
        }

        return regionIndex + 1;
    }
}
