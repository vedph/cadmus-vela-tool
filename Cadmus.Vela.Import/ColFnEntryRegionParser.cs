using Cadmus.Import.Proteus;
using Cadmus.General.Parts;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace Cadmus.Vela.Import;

/// <summary>
/// VeLA column funzione dell'epigrafe/graffito children entries region parser.
/// This targets <see cref="CategoriesPart"/> with role <c>fn</c>.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.vela.col-funzione")]
public sealed class ColFnEntryRegionParser(
    ILogger<ColFnEntryRegionParser>? logger = null) : EntryRegionParser,
    IEntryRegionParser
{
    private const string COL_TESTO = "col-testo";
    private const string COL_MONOGRAMMA = "col-monogramma";
    private const string COL_LETTERA_SINGOLA = "col-lettera_singola";
    private const string COL_LETTERE_NON_INT = "col-lettere_non_interpretabili";
    private readonly ILogger<ColFnEntryRegionParser>? _logger = logger;

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

        return regions[regionIndex].Tag == COL_TESTO ||
            regions[regionIndex].Tag == COL_MONOGRAMMA ||
            regions[regionIndex].Tag == COL_LETTERA_SINGOLA ||
            regions[regionIndex].Tag == COL_LETTERE_NON_INT;
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
            _logger?.LogError("{Tag} column without " +
                "any item at region {Region}", region.Tag, region);
            throw new InvalidOperationException(
                $"{region.Tag} column without any item at " +
                "region " + region);
        }

        DecodedTextEntry txt = (DecodedTextEntry)
            set.Entries[region.Range.Start.Entry + 1];

        if (VelaHelper.GetBooleanValue(txt.Value))
        {
            CategoriesPart part =
                ctx.EnsurePartForCurrentItem<CategoriesPart>("fn");

            string value = region.Tag switch
            {
                COL_TESTO => "testo",
                COL_MONOGRAMMA => "monogramma",
                COL_LETTERA_SINGOLA => "lettera singola",
                COL_LETTERE_NON_INT => "lettere non interpretabili",
                _ => region.Tag!
            };

            string id = VelaHelper.GetThesaurusId(ctx, region,
                VelaHelper.T_CATEGORIES_FN, value, _logger);

            part.Categories.Add(id);
        }

        return regionIndex + 1;
    }
}
