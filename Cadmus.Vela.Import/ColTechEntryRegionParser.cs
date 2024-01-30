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
/// VeLA entry region parser for columns tecnica_di_esecuzione and
/// strumento_di_esecuzione. This targets <see cref="GrfTechniquePart"/>.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.vela.col-tech")]
public sealed class ColTechEntryRegionParser : EntryRegionParser,
    IEntryRegionParser
{
    private readonly ILogger<ColTechEntryRegionParser>? _logger;
    private readonly Dictionary<string, string> _techTags;
    private readonly Dictionary<string, string> _toolTags;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColTechEntryRegionParser"/>
    /// class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public ColTechEntryRegionParser(
        ILogger<ColTechEntryRegionParser>? logger = null)
    {
        _logger = logger;
        _techTags = new Dictionary<string, string>
        {
            ["col-presenza_di_disegno_preparatorio"] = "preparation-drawing",
            ["col-presenza_di_preparazione_del_supporto"] = "preparation-support",
            ["col-graffio"] = "scratch",
            ["col-incisione"] = "engraving",
            ["col-intaglio"] = "carving",
            ["col-disegno"] = "drawing",
            ["col-punzonatura"] = "punching",
            ["col-a_rilievo"] = "relief",
            // "rubricatura" is handled by ColWritingEntryRegionParser
        };
        _toolTags = new Dictionary<string, string>
        {
            ["col-chiodo"] = "nail",
            ["col-gradina"] = "gradine",
            ["col-scalpello"] = "chisel",
            ["col-sgorbia"] = "gouge",
            ["col-sega"] = "saw",
            ["col-bocciarda"] = "bush-hammer",
            ["col-grafite"] = "graphite",
            ["col-matita_di_piombo"] = "lead-pencil",
            ["col-fumo_di_candela"] = "candlesmoke",
            ["col-inchiostro"] = "ink",
            ["col-vernice"] = "paint",
            ["col-lama_(affilatura)"] = "blade",
            // this has custom logic
            ["col-tipo_di_lama"] = ""
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

        return _techTags.ContainsKey(regions[regionIndex].Tag ?? "") ||
               _toolTags.ContainsKey(regions[regionIndex].Tag ?? "");
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

        string? value = VelaHelper.FilterValue(txt.Value, true);

        if (!string.IsNullOrEmpty(value))
        {
            GrfTechniquePart part =
                ctx.EnsurePartForCurrentItem<GrfTechniquePart>();

            if (_techTags.TryGetValue(region.Tag!, out string? id))
            {
                part.Techniques.Add(id);
            }
            else
            {
                if (region.Tag == "col-tipo_di_lama")
                {
                    // non empty values are only "lama curva" or "lama dritta"
                    switch (value)
                    {
                        case "lama curva":
                            part.Tools.Add("curved-blade");
                            break;
                        case "lama dritta":
                            part.Tools.Add("straight-blade");
                            break;
                        default:
                            part.Tools.Add(value);
                            _logger?.LogError("Unknown blade type: {value} " +
                                "at region {region}", value, region);
                            break;
                    }
                }
                else
                {
                    part.Tools.Add(_toolTags[region.Tag!]);
                }
            }
        }

        return regionIndex + 1;
    }
}
