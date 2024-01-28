using Cadmus.General.Parts;
using Cadmus.Import.Proteus;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;

namespace Cadmus.Vela.Import;

/// <summary>
/// VeLA columns figurativi, testo, numeri, cornice entry region parser. This
/// targets <see cref="CategoriesPart"/> with role <c>features</c>.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.vela.col-features")]
public sealed class ColFeaturesEntryRegionParser : EntryRegionParser,
    IEntryRegionParser
{
    private readonly ILogger<ColFeaturesEntryRegionParser>? _logger;
    private readonly HashSet<string> _tags;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColFeaturesEntryRegionParser"/>
    /// class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public ColFeaturesEntryRegionParser(
        ILogger<ColFeaturesEntryRegionParser>? logger = null)
    {
        _logger = logger;
        _tags =
        [
            "col-figurativi", "col-testo", "col-numeri", "col-cornice"
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
            // ID from thesaurus categories_features
            string? id = null;
            switch (region.Tag)
            {
                case "col-figurativi":
                    id = "figurative";
                    break;
                case "col-testo":
                    id = "text";
                    break;
                case "col-numeri":
                    id = "digits";
                    break;
                case "col-cornice":
                    id = "frame";
                    break;
            }

            if (id != null)
            {
                CategoriesPart part =
                    ctx.EnsurePartForCurrentItem<CategoriesPart>();
                part.Categories.Add(id);
            }
        }

        return regionIndex + 1;
    }
}
