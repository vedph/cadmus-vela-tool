using Cadmus.General.Parts;
using Cadmus.Import.Proteus;
using Fusi.Antiquity.Chronology;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;

namespace Cadmus.Vela.Import;

/// <summary>
/// VeLA column cronologia entry region parser. This targets
/// <see cref="HistoricalDatePart"/>.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.vela.col-cronologia")]
public sealed class ColDatationEntryRegionParser : EntryRegionParser,
    IEntryRegionParser
{
    private readonly ILogger<ColDatationEntryRegionParser>? _logger;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="ColDatationEntryRegionParser"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public ColDatationEntryRegionParser(
        ILogger<ColDatationEntryRegionParser>? logger = null)
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

        return regions[regionIndex].Tag == "col-cronologia";
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
                "cronologia column without any item at region {region}",
                regions[regionIndex]);
            throw new InvalidOperationException(
                "cronologia column without any item at region " +
                regions[regionIndex]);
        }

        DecodedTextEntry txt = (DecodedTextEntry)
            set.Entries[region.Range.Start.Entry + 1];
        string? value = VelaHelper.FilterValue(txt.Value, false);

        // cronologia is just a copy of terminus ante/post if they are present,
        // so in this case just ignore it
        HistoricalDatePart part =
            ctx.EnsurePartForCurrentItem<HistoricalDatePart>();

        if (part.Date is null ||
            part.Date.GetDateType() == HistoricalDateType.Undefined)
        {
            part.Date = HistoricalDate.Parse(value);
        }

        return regionIndex + 1;
    }
}
