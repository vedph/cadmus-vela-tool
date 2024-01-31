using Cadmus.Import.Proteus;
using Cadmus.Vela.Parts;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;

namespace Cadmus.Vela.Import;

/// <summary>
/// VeLA material column entry region parser. This targets
/// <see cref="GrfSupportPart.Material"/>.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.vela.col-materiale")]
public sealed class ColMatTypeEntryRegionParser : EntryRegionParser,
    IEntryRegionParser
{
    private readonly ILogger<ColMatTypeEntryRegionParser>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColMatTypeEntryRegionParser"/>
    /// class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public ColMatTypeEntryRegionParser(
        ILogger<ColMatTypeEntryRegionParser>? logger = null)
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

        return regions[regionIndex].Tag == "col-materiale";
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
            _logger?.LogError("materiale column without any " +
                "item at region {region}", region);
            throw new InvalidOperationException(
                "materiale column without any item at region " +
                region);
        }

        DecodedTextEntry txt = (DecodedTextEntry)
            set.Entries[region.Range.Start.Entry + 1];
        string? value = VelaHelper.FilterValue(txt.Value, true);
        string? id = value != null
            ? ctx.ThesaurusEntryMap!.GetEntryId(
                VelaHelper.T_GRF_SUPPORT_MATERIALS, value)
            : null;

        if (id == null)
        {
            _logger?.LogError("Unknown value for materiale: \"{value}\" " +
                "at region {region}", value, region);
            id = value;
        }

        GrfSupportPart part =
            ctx.EnsurePartForCurrentItem<GrfSupportPart>();
        part.Material = id;

        return regionIndex + 1;
    }
}
