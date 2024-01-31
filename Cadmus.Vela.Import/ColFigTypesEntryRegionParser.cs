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
    private readonly Dictionary<string, string> _tags;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColFigTypesEntryRegionParser"/>
    /// class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public ColFigTypesEntryRegionParser(
        ILogger<ColFigTypesEntryRegionParser>? logger = null)
    {
        _logger = logger;
        _tags = new Dictionary<string, string>
        {
            ["col-parti_anatomiche"] = "anatomical",
            ["col-volti"] = "face",
            ["col-busto"] = "bust",
            ["col-figura_umana"] = "human-fig",
            ["col-erotici"] = "erotic",
            ["col-croce"] = "cross",
            ["col-cuore"] = "heart",
            ["col-architettura"] = "architecture",
            ["col-paesaggi"] = "landscape",
            ["col-geometrico"] = "geometric",
            ["col-imbarcazioni"] = "ship",
            ["col-piante"] = "plant",
            ["col-gioco"] = "game",
            ["col-arma"] = "weapon",
            ["col-armatura"] = "armor",
            ["col-stemma"] = "coat-of-arms",
            ["col-bandiera"] = "flag",
            ["col-animale"] = "animal",
            ["col-simbolo_zodiaco"] = "zodiac",
            ["col-graffito_da_affilitura"] = "sharpening"
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

        return _tags.ContainsKey(regions[regionIndex].Tag ?? "");
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
            GrfFigurativePart part =
                ctx.EnsurePartForCurrentItem<GrfFigurativePart>();
            part.Types.Add(_tags[region.Tag!]);
        }

        return regionIndex + 1;
    }
}
