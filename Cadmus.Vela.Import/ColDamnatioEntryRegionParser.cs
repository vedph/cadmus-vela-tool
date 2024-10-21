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
/// VeLA column presenza_di_damnatio entry region parser. This targets
/// <see cref="GrfLocalizationPart.Damnatio"/>.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.vela.col-presenza_di_damnatio")]
public sealed class ColDamnatioEntryRegionParser : EntryRegionParser,
    IEntryRegionParser
{
    private readonly ILogger<ColDamnatioEntryRegionParser>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColDamnatioEntryRegionParser"/>
    /// class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public ColDamnatioEntryRegionParser(
        ILogger<ColDamnatioEntryRegionParser>? logger = null)
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

        return regions[regionIndex].Tag == "col-presenza_di_damnatio";
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
            _logger?.LogError("presenza_di_damnatio column without any item " +
                "at region {Region}", region);
            throw new InvalidOperationException(
                "presenza_di_damnatio column without any item at region "
                + region);
        }

        DecodedTextEntry txt = (DecodedTextEntry)
            set.Entries[region.Range.Start.Entry + 1];
        string? value = VelaHelper.FilterValue(txt.Value, false);

        if (string.IsNullOrEmpty(value)) return regionIndex + 1;

        GrfLocalizationPart part =
            ctx.EnsurePartForCurrentItem<GrfLocalizationPart>();

        switch (value)
        {
            case "non presente":
                part.Damnatio = "absent";
                break;
            case "parziale":
                part.Damnatio = "partial";
                break;
            case "totale":
                part.Damnatio = "complete";
                break;
            default:
                _logger?.LogError("Unknown presenza_di_damnatio value {Value} " +
                    "at region {Region}", value, region);
                part.Damnatio = value;
                break;
        }

        return regionIndex + 1;
    }
}
