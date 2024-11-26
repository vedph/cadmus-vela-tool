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
/// VeLA entry region parser for columns scrittura,
/// tipologia_grafica_caratteri_latini, and children of segni_grafici_particolari.
/// This targets <see cref="EpiWritingPart"/>.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.vela.col-writing")]
public sealed class ColWritingEntryRegionParser(
    ILogger<ColWritingEntryRegionParser>? logger = null) : EntryRegionParser,
    IEntryRegionParser
{
    private const string COL_SCRITTURA = "col-scrittura";
    private const string COL_TIPOLOGIA_GRAFICA = "col-tipologia_grafica_caratteri_latini";
    private const string COL_ABBREVIAZIONI = "col-abbreviazioni";
    private const string COL_NESSI = "col-nessi_e_legamenti";
    private const string COL_LETTERE_INCLUSE = "col-lettere_incluse";
    private const string COL_LETTERE_SOVVRAPPOSTE = "col-lettere_sovrapposte";
    private const string COL_PUNTEGGIATURA = "col-punteggiatura";
    private const string COL_SEGNI_INTERPUNZIONE = "col-segni_di_interpunzione";

    private readonly ILogger<ColWritingEntryRegionParser>? _logger = logger;
    private readonly HashSet<string> _colNames =
    [
        COL_SCRITTURA,
        COL_TIPOLOGIA_GRAFICA,
        COL_ABBREVIAZIONI,
        COL_NESSI,
        COL_LETTERE_INCLUSE,
        COL_LETTERE_SOVVRAPPOSTE,
        COL_PUNTEGGIATURA,
        COL_SEGNI_INTERPUNZIONE
    ];

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

        return _colNames.Contains(regions[regionIndex].Tag ?? "");
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
        if (string.IsNullOrEmpty(value)) return regionIndex + 1;

        EpiWritingPart part = ctx.EnsurePartForCurrentItem<EpiWritingPart>();

        switch (region.Tag)
        {
            case COL_SCRITTURA:
                part.Casing = VelaHelper.GetThesaurusId(ctx, region,
                    VelaHelper.T_EPI_WRITING_CASINGS, value, _logger);
                break;
            case COL_TIPOLOGIA_GRAFICA:
                part.Script = VelaHelper.GetThesaurusId(ctx, region,
                    VelaHelper.T_EPI_WRITING_SCRIPTS, value, _logger);
                break;
            default:
                if (VelaHelper.GetBooleanValue(txt.Value))
                {
                    string col = region.Tag![4..].Replace('_', ' ');
                    part.Features.Add(VelaHelper.GetThesaurusId(ctx, region,
                        VelaHelper.T_EPI_WRITING_FEATURES, col, _logger));
                }
                break;
        }

        return regionIndex + 1;
    }
}
