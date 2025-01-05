using Cadmus.Import.Proteus;
using Cadmus.Refs.Bricks;
using Cadmus.General.Parts;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;

namespace Cadmus.Vela.Import;

/// <summary>
/// VeLA column area, sestiere and denominazione entry region parser. This
/// targets a <see cref="DistrictLocationPart"/> for many columns building up
/// a district-based location. Some of these columns have closed value sets,
/// from the corresponding thesaurus, while others are free.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.vela.col-area")]
public sealed class ColAreaEntryRegionParser(
    ILogger<ColAreaEntryRegionParser>? logger = null) : EntryRegionParser,
    IEntryRegionParser
{
    private const string COL_PROVINCIA = "col-provincia";
    private const string COL_CITTA = "col-citta'";
    private const string COL_CENTER = "col-centri/localita'";
    private const string COL_CENTER_ALIAS = "col-centri/località";
    private const string COL_LOCATION = "col-localizzazione";
    private const string COL_STRUTTURA = "col-denominazione_struttura";
    private readonly ILogger<ColAreaEntryRegionParser>? _logger = logger;

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

        return regions[regionIndex].Tag == COL_PROVINCIA ||
            regions[regionIndex].Tag == COL_CITTA ||
            regions[regionIndex].Tag == COL_CENTER ||
            regions[regionIndex].Tag == COL_CENTER_ALIAS ||
            regions[regionIndex].Tag == COL_LOCATION ||
            regions[regionIndex].Tag == COL_STRUTTURA;
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
        string? value = VelaHelper.FilterValue(txt.Value, false);
        if (value == null)
        {
            _logger?.LogWarning("{Tag} column with empty value at region {Region}",
                region.Tag, region);
            return regionIndex + 1;
        }

        DistrictLocationPart part =
            ctx.EnsurePartForCurrentItem<DistrictLocationPart>();
        part.Place ??= new ProperName();
        part.Place.Language = "ita";

        bool hasFreeValue = region.Tag == COL_CITTA ||
            region.Tag == COL_CENTER ||
            region.Tag == COL_LOCATION ||
            region.Tag == COL_STRUTTURA;

        string? pieceValue = hasFreeValue
                ? value
                : VelaHelper.GetThesaurusId(ctx, region,
                    VelaHelper.T_DISTRICT_NAME_PIECE_TYPES, value, _logger);

        if (!string.IsNullOrEmpty(pieceValue))
        {
            part.Place.Pieces!.Add(new ProperNamePiece
            {
                Type = region.Tag switch
                {
                    COL_PROVINCIA => "p*",
                    COL_CITTA => "c*",
                    COL_CENTER => "e*",
                    COL_CENTER_ALIAS => "e*",
                    COL_LOCATION => "l*",
                    COL_STRUTTURA => "s*",
                    _ => throw new InvalidOperationException(
                        $"Unexpected \"{value}\" in region {region.Tag}")
                },
                Value = pieceValue
            });
        }

        return regionIndex + 1;
    }
}
