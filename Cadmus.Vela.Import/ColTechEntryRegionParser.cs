using Cadmus.Import.Proteus;
using Cadmus.Epigraphy.Parts;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;

namespace Cadmus.Vela.Import;

/// <summary>
/// VeLA entry region parser for columns tecnica_di_esecuzione and
/// strumento_di_esecuzione. This targets <see cref="EpiTechniquePart.Techniques"/>
/// and <see cref="EpiTechniquePart.Tools"/>.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.vela.col-tech")]
public sealed class ColTechEntryRegionParser(
    ILogger<ColTechEntryRegionParser>? logger = null) : EntryRegionParser,
    IEntryRegionParser
{
    private const string COL_TECNICA = "col-tecnica_di_esecuzione";
    private const string COL_STRUMENTO = "col-strumento_di_esecuzione";
    private readonly ILogger<ColTechEntryRegionParser>? _logger = logger;

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

        return regions[regionIndex].Tag == COL_TECNICA ||
               regions[regionIndex].Tag == COL_STRUMENTO;
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
            _logger?.LogError("{Tag} column without any item at region {Region}",
                region.Tag, region);
            throw new InvalidOperationException(
                $"{region.Tag} column without any item at region {region}");
        }

        DecodedTextEntry txt = (DecodedTextEntry)
            set.Entries[region.Range.Start.Entry + 1];

        string? value = VelaHelper.FilterValue(txt.Value, true);

        if (!string.IsNullOrEmpty(value))
        {
            EpiTechniquePart part =
                ctx.EnsurePartForCurrentItem<EpiTechniquePart>();

            switch (region.Tag)
            {
                case COL_TECNICA:
                    string id = VelaHelper.GetThesaurusId(ctx, region,
                        VelaHelper.T_EPI_TECHNIQUE_TYPES, value, _logger);
                    part.Techniques.Add(id);
                    break;
                case COL_STRUMENTO:
                    id = VelaHelper.GetThesaurusId(ctx, region,
                        VelaHelper.T_EPI_TECHNIQUE_TOOLS, value, _logger);
                    part.Tools.Add(id);
                    break;
            }
        }

        return regionIndex + 1;
    }
}
