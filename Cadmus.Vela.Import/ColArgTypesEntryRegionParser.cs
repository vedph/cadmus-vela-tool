using Cadmus.General.Parts;
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
/// VeLA argument types columns entry region parser. This targets various
/// parts according to the column in this section.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.vela.col-arg-types")]
public sealed class ColArgTypesEntryRegionParser : EntryRegionParser,
    IEntryRegionParser
{
    private readonly ILogger<ColArgTypesEntryRegionParser>? _logger;
    private readonly Dictionary<string, string> _fnTags;
    private readonly Dictionary<string, string> _thTags;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColArgTypesEntryRegionParser"/>
    /// class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public ColArgTypesEntryRegionParser(
        ILogger<ColArgTypesEntryRegionParser>? logger = null)
    {
        _logger = logger;
        _fnTags = new Dictionary<string, string>
        {
            ["col-funeraria"] = "funerary",
            ["col-commemorativa"] = "memorial",
            ["col-firma"] = "signature",
            ["col-celebrativa"] = "celebratory",
            ["col-esortativa"] = "exhortative",
            ["col-didascalica"] = "didascalic",
            ["col-segnaletica"] = "signaling",
            ["col-parlanti"] = "speaking"
        };

        _thTags = new Dictionary<string, string>
        {
            ["col-iniziali\\i_nome_persona"] = "initials",
            ["col-citazione"] = "quote",
            ["col-infamante"] = "infamous",
            ["col-sport"] = "sport",
            ["col-prostituzione"] = "prostitution",
            ["col-politica"] = "politics",
            ["col-religiosa"] = "religion",
            ["col-preghiera"] = "prayer",
            ["col-ex_voto"] = "ex-voto",
            ["col-amore"] = "love",
            ["col-insulto"] = "taunt",
            ["col-imprecazioni"] = "curse",
            ["col-nome_di_luogo"] = "toponym",
            ["col-saluti"] = "greeting"
        };
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

        string tag = regions[regionIndex].Tag ?? "";

        return tag == "col-sigla" || tag == "col-poesia" ||
               _fnTags.ContainsKey(tag) || _thTags.ContainsKey(tag);
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
            _logger?.LogError("{tag} column without any item at region {region}",
                region.Tag, region);
            throw new InvalidOperationException(
                $"{region.Tag} column without any item at region {region}");
        }

        DecodedTextEntry txt = (DecodedTextEntry)
            set.Entries[region.Range.Start.Entry + 1];
        bool value = VelaHelper.GetBooleanValue(txt.Value);
        if (!value) return regionIndex + 1;

        if (_fnTags.TryGetValue(region.Tag!, out string? fid))
        {
            CategoriesPart part =
                ctx.EnsurePartForCurrentItem<CategoriesPart>("functions");
            part.Categories.Add(fid);
        }
        else if (_thTags.TryGetValue(region.Tag!, out string? tid))
        {
            CategoriesPart part =
                ctx.EnsurePartForCurrentItem<CategoriesPart>("themes");
            part.Categories.Add(tid);
        }
        else
        {
            switch (region.Tag)
            {
                case "col-sigla":
                    ctx.EnsurePartForCurrentItem<GrfWritingPart>()
                        .LetterFeatures.Add("sigla");
                    break;

                case "col-prosa":
                    ctx.EnsurePartForCurrentItem<GrfWritingPart>()
                        .HasProse = true;
                    break;

                case "col-poesia":
                    ctx.EnsurePartForCurrentItem<GrfWritingPart>()
                        .HasPoetry = true;
                    break;
            }
        }

        return regionIndex + 1;
    }
}
