using Cadmus.Import.Proteus;
using Cadmus.Refs.Bricks;
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
    private readonly HashSet<string> _fnTags;
    private readonly HashSet<string> _thTags;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColArgTypesEntryRegionParser"/>
    /// class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public ColArgTypesEntryRegionParser(
        ILogger<ColArgTypesEntryRegionParser>? logger = null)
    {
        _logger = logger;
        _fnTags =
            [
                "col-funeraria", "col-commemorativa", "col-firma",
                "col-celebrativa", "col-esortativa", "col-didascalica",
                "col-segnaletica", "col-parlanti"
            ];
        _thTags =
            [
                "col-iniziali\\i_nome_persona", "col-citazione", "col-infamante",
                "col-sport", "col-prostituzione", "col-politica",
                "col-religiosa", "col-preghiera", "col-ex_voto", "col-amore",
                "col-insulto", "col-imprecazioni", "col-nome_di_luogo",
                "col-saluti"
            ];
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
               _fnTags.Contains(tag) || _thTags.Contains(tag);
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

        if (_fnTags.Contains(region.Tag!))
        {
            // TODO
        }
        else if (_thTags.Contains(region.Tag!))
        {
            // TODO
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
