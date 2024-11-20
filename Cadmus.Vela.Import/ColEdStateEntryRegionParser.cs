using Cadmus.Import.Proteus;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;

namespace Cadmus.Vela.Import;

/// <summary>
/// VeLA column stato entry region parser. This targets the editorial state
/// of the record as defined by item's flags.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.vela.col-stato")]
public sealed class ColEdStateEntryRegionParser(
    ILogger<ColEdStateEntryRegionParser>? logger = null) : EntryRegionParser,
    IEntryRegionParser
{
    private const string COL_STATE = "col-stato";
    private readonly ILogger<ColEdStateEntryRegionParser>? _logger = logger;

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

        return regions[regionIndex].Tag == COL_STATE;
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
            _logger?.LogError("stato column without any item at region {Region}",
                region);
            throw new InvalidOperationException(
                "stato column without any item at region " + region);
        }

        DecodedTextEntry txt = (DecodedTextEntry)
            set.Entries[region.Range.Start.Entry + 1];
        switch (VelaHelper.FilterValue(txt.Value, true))
        {
            case "in lavorazione":
                ctx.CurrentItem.Flags |= 1;
                break;
            case "importata":
                ctx.CurrentItem.Flags |= 2;
                break;
            case "lavorata":
                ctx.CurrentItem.Flags |= 4;
                break;
            case "rilevata":
                ctx.CurrentItem.Flags |= 8;
                break;
            // this is usually found in another column but to be more robust
            // we also check for it here
            case "convalidata":
                ctx.CurrentItem.Flags |= 0x10;
                break;
        }

        return regionIndex + 1;
    }
}
