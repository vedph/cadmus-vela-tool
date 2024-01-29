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
/// VeLA column figurative types columns entry region parser. This targets
/// <see cref="GrfFigurativePart.Types"/>.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.vela.col-fig-types")]
public sealed class ColFigTypesEntryRegionParser : EntryRegionParser,
    IEntryRegionParser
{
    private readonly ILogger<ColFigTypesEntryRegionParser>? _logger;
    private readonly HashSet<string> _tags;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColFigTypesEntryRegionParser"/>
    /// class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public ColFigTypesEntryRegionParser(
        ILogger<ColFigTypesEntryRegionParser>? logger = null)
    {
        _logger = logger;
        _tags =
            [
                "col-parti_anatomiche", "col-volti", "col-busto",
                "col-figura_umana", "col-erotici", "col-croce", "col-cuore",
                "col-architettura", "col-paesaggi", "col-geometrico",
                "col-imbarcazioni", "col-piante", "col-gioco", "col-arma",
                "col-armatura", "col-stemma", "col-bandiera", "col-animale",
                "col-simbolo_zodiaco", "col-graffito_da_affilitura"
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

        return _tags.Contains(regions[regionIndex].Tag ?? "");
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

        if (VelaHelper.GetBooleanValue(txt.Value))
        {
            // ID from thesaurus grf_figurative_types
            string? id = null;
            switch (region.Tag)
            {
                case "col-parti_anatomiche":
                    id = "hum.anatomical";
                    break;
                case "col-volti":
                    id = "hum.face";
                    break;
                case "col-busto":
                    id = "hum.bust";
                    break;
                case "col-figura_umana":
                    id = "hum.-";
                    break;
                case "col-erotici":
                    id = "erotic";
                    break;
                case "col-croce":
                    id = "sym.cross";
                    break;
                case "col-cuore":
                    id = "sym.heart";
                    break;
                case "col-architettura":
                    id = "architecture";
                    break;
                case "col-paesaggi":
                    id = "landscape";
                    break;
                case "col-geometrico":
                    id = "geometric";
                    break;
                case "col-imbarcazioni":
                    id = "transport.boat";
                    break;
                case "col-piante":
                    id = "plant";
                    break;
                case "col-gioco":
                    id = "game";
                    break;
                case "col-arma":
                    id = "war.weapon";
                    break;
                case "col-armatura":
                    id = "war.armor";
                    break;
                case "col-stemma":
                    id = "coat-of-arms";
                    break;
                case "col-bandiera":
                    id = "flag";
                    break;
                case "col-animale":
                    id = "ani";
                    break;
                case "col-simbolo_zodiaco":
                    id = "sym.zodiac";
                    break;
                case "col-graffito_da_affilitura":
                    id = "sharpening";
                    break;
            }

            if (id != null)
            {
                GrfFigurativePart part =
                    ctx.EnsurePartForCurrentItem<GrfFigurativePart>();
                part.Types.Add(id);
            }
        }

        return regionIndex + 1;
    }
}
