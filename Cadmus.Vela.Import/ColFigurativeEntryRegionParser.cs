using Cadmus.Import.Proteus;
using Cadmus.General.Parts;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;

namespace Cadmus.Vela.Import;

/// <summary>
/// VeLA column figurativo children entries region parser. This targets
/// <see cref="CategoriesPart"/> with role <c>fig</c>.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.vela.col-figurativo")]
public sealed class ColFigurativeEntryRegionParser(
    ILogger<ColFigurativeEntryRegionParser>? logger = null) : EntryRegionParser,
    IEntryRegionParser
{
    private readonly ILogger<ColFigurativeEntryRegionParser>? _logger = logger;
    private readonly HashSet<string> _colNames =
    [
        "col-disegno_non_interpretabile", "col-abbigliamento", "col-animale",
        "col-architettura", "col-arma", "col-armatura", "col-bandiera",
        "col-busto", "col-croce", "col-cuore", "col-erotico", "col-figura_umana",
        "col-geometrico", "col-gioco", "col-imbarcazione", "col-lingua",
        "col-paesaggio", "col-pianta", "col-simbolo_zodiacale", "col-sistema",
        "col-volto"
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

        if (VelaHelper.GetBooleanValue(txt.Value))
        {
            // ID from thesaurus grf_figurative_types
            GrfFigurativePart part =
                ctx.EnsurePartForCurrentItem<GrfFigurativePart>();
            part.Types.Add(_tags[region.Tag!]);
        }

        return regionIndex + 1;
    }
}
