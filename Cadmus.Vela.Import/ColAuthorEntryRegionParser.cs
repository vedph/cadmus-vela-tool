using Cadmus.General.Parts;
using Cadmus.Import.Proteus;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cadmus.Vela.Import;

/// <summary>
/// VeLA column author entry region parser. This targets the metadata part.
/// The author column contains 1 or more authors, separated by comma. Each
/// author is added as an author entry to the metadata part of the current item.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <param name="logger">The logger.</param>
[Tag("entry-region-parser.vela.col-autore")]
public sealed class ColAuthorEntryRegionParser(
    ILogger<ColAuthorEntryRegionParser>? logger = null) : EntryRegionParser,
    IEntryRegionParser
{
    private const string COL_AUTHOR = "autore";
    private readonly ILogger<ColAuthorEntryRegionParser>? _logger = logger;

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

        return regions[regionIndex].Tag == COL_AUTHOR;
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
            _logger?.LogError("author column without any item at region {Region}",
                region);
            throw new InvalidOperationException(
                "author column without any item at region " + region);
        }

        // get the text entry value
        DecodedTextEntry txt = (DecodedTextEntry)
            set.Entries[region.Range.Start.Entry + 1];
        string? value = VelaHelper.FilterValue(txt.Value, false);

        if (!string.IsNullOrEmpty(value))
        {
            MetadataPart part = ctx.EnsurePartForCurrentItem<MetadataPart>();

            // split authors by comma (using hashset to avoid duplicates)
            HashSet<string> authors = new(VelaHelper.GetValueList(value, false));

            // add each author
            foreach (string author in authors.Where(a => a.Length > 0))
            {
                part.Metadata.Add(new Metadatum
                {
                    Name = "author",
                    Value = author,
                });
            }
        }
        return regionIndex + 1;
    }
}
