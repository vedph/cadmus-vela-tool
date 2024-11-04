using Cadmus.Import.Proteus;
using Cadmus.Epigraphy.Parts;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;
using Cadmus.Refs.Bricks;

namespace Cadmus.Vela.Import;

/// <summary>
/// VeLA column entry region parser for children of column impaginazione_del_testo.
/// This targets <see cref="EpiSupportPart.Features"/>,
/// <see cref="EpiSupportPart.Counts"/>, <see cref="EpiSupportPart.Note"/>.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.vela.col-impaginazione")]
public sealed class ColLayoutEntryRegionParser(
    ILogger<ColLayoutEntryRegionParser>? logger = null) : EntryRegionParser,
    IEntryRegionParser
{
    private const string COL_RIGATURA = "col-rigatura";
    private const string COL_NUMERO_RIGHE = "col-numero_righe";
    private const string COL_NOTE = "col-note";
    private const string COL_PREPARAZIONE =
        "col-presenza_di_preparazione_del_supporto";
    private readonly ILogger<ColLayoutEntryRegionParser>? _logger = logger;

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

        return regions[regionIndex].Tag == COL_RIGATURA ||
            regions[regionIndex].Tag == COL_NUMERO_RIGHE ||
            regions[regionIndex].Tag == COL_NOTE ||
            regions[regionIndex].Tag == COL_PREPARAZIONE;
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
                $"{region.Tag} column without any item at region " + region);
        }

        DecodedTextEntry txt = (DecodedTextEntry)
            set.Entries[region.Range.Start.Entry + 1];
        string? value = VelaHelper.FilterValue(txt.Value, false);

        if (!string.IsNullOrEmpty(value))
        {
            EpiSupportPart part = ctx.EnsurePartForCurrentItem<EpiSupportPart>();

            switch (region.Tag)
            {
                case COL_RIGATURA:
                    if (VelaHelper.GetBooleanValue(txt.Value))
                        part.Features.Add("ruling");
                    break;
                case COL_NUMERO_RIGHE:
                    part.Counts.Add(new DecoratedCount
                    {
                        Id = "rows",
                        Value = VelaHelper.GetIntValue(value)
                    });
                    break;
                case COL_NOTE:
                    part.Note = value;
                    break;
                case COL_PREPARAZIONE:
                    if (VelaHelper.GetBooleanValue(txt.Value))
                        part.Features.Add("preparation");
                    break;
            }
        }

        return regionIndex + 1;
    }
}
