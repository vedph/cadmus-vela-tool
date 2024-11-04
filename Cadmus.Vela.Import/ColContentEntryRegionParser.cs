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
/// VeLA column contenuto + cifra entry region parser. This targets children
/// entries, mapping them into thesaurus entries of a <see cref="CategoriesPart"/>
/// with role=<c>cnt</c>.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.vela.col-contenuto")]
public sealed class ColContentEntryRegionParser(
    ILogger<ColContentEntryRegionParser>? logger = null) : EntryRegionParser,
    IEntryRegionParser
{
    private readonly ILogger<ColContentEntryRegionParser>? _logger = logger;
    private readonly HashSet<string> _colNames =
        [
            "col-amore", "col-augurale", "col-autentica_di_reliquie",
            "col-bollo laterizio", "col-calendario", "col-celebrativa",
            "col-citazione", "col-commemorativa", "col-consacrazione",
            "col-dedicatoria", "col-devozionale", "col-didascalica",
            "col-documentaria", "col-esegetica", "col-esortativa", "col-ex_voto",
            "col-firma", "col-funeraria", "col-imprecazione", "col-infamante",
            "col-iniziale\\i_nome_persona", "col-insulto", "col-invocativa",
            "col-marchio_edile", "col-nome", "col-nome di luogo",
            "col-parlante", "col-politica", "col-poesia", "col-prosa",
            "col-prostituzione", "col-preghiera", "col-religiosa", "col-saluto",
            "col-segnaletica", "col-sigla", "col-sport",
            "col-funzione_non_definibile", "cifra"
        ];

    // this is a direct mapping between column name and thesaurus values,
    // for those column names which are not equal to the corresponding thesaurus
    // value (e.g. "inizialie\i nome persona" => "iniziali nome")
    private readonly Dictionary<string, string> _colValueMap = new()
    {
        ["iniziali\\e nome persona"] = "iniziali nome",
        ["funzione non definibile"] = "non definibile"
    };

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
            _logger?.LogError("__TAG__ column without any item at region {Region}",
                region);
            throw new InvalidOperationException(
                "__TAG__ column without any item at region " + region);
        }

        DecodedTextEntry txt = (DecodedTextEntry)
            set.Entries[region.Range.Start.Entry + 1];
        string? value = VelaHelper.FilterValue(txt.Value, false);
        if (value != null)
        {
            string col = _colValueMap.TryGetValue(region.Tag!, out string? v)
                ? v : region.Tag!;
            string id;

            if (col == "cifra")
            {
                switch (value)
                {
                    case "araba":
                        id = "digit.ar";
                        break;
                    case "armena":
                        id = "digit.hy";
                        break;
                    case "cirillica":
                        id = "digit.ru";
                        break;
                    case "glagolitica":
                        id = "digit.sla";
                        break;
                    case "romana":
                        id = "digit.la";
                        break;
                    default:
                        id = "";
                        Logger?.LogError("Unknown cifra value: " +
                            "{Value} in region {Region}", value, region);
                        break;
                }
            }
            else
            {
                id = VelaHelper.GetThesaurusId(ctx, region,
                    VelaHelper.T_CATEGORIES_CNT, col, _logger);
            }
            if (id.Length > 0)
            {
                CategoriesPart part =
                    ctx.EnsurePartForCurrentItem<CategoriesPart>("cnt");
                part.Categories.Add(id);
            }
        }

        return regionIndex + 1;
    }
}
