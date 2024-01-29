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
    private readonly HashSet<string> _techTags;
    private readonly HashSet<string> _toolTags;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColTechEntryRegionParser"/>
    /// class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public ColTechEntryRegionParser(
        ILogger<ColTechEntryRegionParser>? logger = null)
    {
        _logger = logger;
        _techTags =
            [
                "col-presenza_di_disegno_preparatorio",
                "col-presenza_di_preparazione_del_supporto",
                "col-graffio",
                "col-incisione",
                "col-intaglio",
                "col-disegno",
                "col-punzonatura",
                // "rubricatura" this is handled for GrfWriting
                "col-a_rilievo",
            ];
        _toolTags =
            [
                "col-chiodo",
                "col-gradina",
                "col-scalpello",
                "col-sgorbia",
                "col-sega",
                "col-bocciarda",
                "col-grafite",
                "col-matita_di_piombo",
                "col-fumo_di_candela",
                "col-inchiostro",
                "col-vernice",
                "col-lama_(affilatura)",
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

        return _techTags.Contains(regions[regionIndex].Tag ?? "") ||
               _toolTags.Contains(regions[regionIndex].Tag ?? "");
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

            if (_techTags.Contains(region.Tag!))
            {
                switch (region.Tag)
                {
                    case "col-presenza_di_disegno_preparatorio":
                        part.Techniques.Add("preparation-drawing");
                        break;
                    case "col-presenza_di_preparazione_del_supporto":
                        part.Techniques.Add("preparation-support");
                        break;
                    case "col-graffio":
                        part.Techniques.Add("scratch");
                        break;
                    case "col-incisione":
                        part.Techniques.Add("engraving");
                        break;
                    case "col-intaglio":
                        part.Techniques.Add("carving");
                        break;
                    case "col-disegno":
                        part.Techniques.Add("drawing");
                        break;
                    case "col-punzonatura":
                        part.Techniques.Add("punching");
                        break;
                    case "col-a_rilievo":
                        part.Techniques.Add("relief");
                        break;
                }
            }
            else
            {
                switch (region.Tag)
                {
                    case "col-chiodo":
                        part.Tools.Add("nail");
                        break;
                    case "col-gradina":
                        part.Tools.Add("gradine");
                        break;
                    case "col-scalpello":
                        part.Tools.Add("chisel");
                        break;
                    case "col-sgorbia":
                        part.Tools.Add("gouge");
                        break;
                    case "col-sega":
                        part.Tools.Add("saw");
                        break;
                    case "col-bocciarda":
                        part.Tools.Add("bush-hammer");
                            break;
                    case "col-grafite":
                        part.Tools.Add("graphite");
                        break;
                    case "col-matita_di_piombo":
                        part.Tools.Add("lead-pencil");
                        break;
                    case "col-fumo_di_candela":
                        part.Tools.Add("candlesmoke");
                        break;
                    case "col-inchiostro":
                        part.Tools.Add("ink");
                        break;
                    case "col-vernice":
                        part.Tools.Add("paint");
                        break;
                    case "col-lama_(affilatura)":
                        part.Tools.Add("blade");
                        break;
                }
            }
        }

        return regionIndex + 1;
    }
}
