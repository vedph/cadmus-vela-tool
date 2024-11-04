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
/// VeLA column terminus_ante entry region parser. This targets
/// <see cref="HistoricalDatePart"/>.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.vela.col-terminus_ante")]
public sealed class ColTerminusAnteEntryRegionParser(
    ILogger<ColTerminusAnteEntryRegionParser>? logger = null) : EntryRegionParser,
    IEntryRegionParser
{
    private readonly ILogger<ColTerminusAnteEntryRegionParser>? _logger = logger;

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

        return regions[regionIndex].Tag == "col-terminus_ante";
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
                "terminus_ante column without any item at region {Region}",
                region);
            throw new InvalidOperationException(
                "terminus_ante column without any item at region " +
                region);
        }

        DecodedTextEntry txt = (DecodedTextEntry)
            set.Entries[region.Range.Start.Entry + 1];
        string? value = VelaHelper.FilterValue(txt.Value, false)
            ?.Replace(" SECOLO", "");

        // terminus ante may come after a terminus post: in this case we have
        // a range, else just a terminus ante
        HistoricalDatePart part =
            ctx.EnsurePartForCurrentItem<HistoricalDatePart>();

        if (part.Date is not null &&
            part.Date.GetDateType() == HistoricalDateType.Range)
        {
            part.Date.B = Datation.Parse(value);
        }
        else
        {
            part.Date = HistoricalDate.Parse($"- {value}");
        }

        return regionIndex + 1;
    }
}
