using Cadmus.Import.Proteus;
using Cadmus.Epigraphy.Parts;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;

namespace Cadmus.Vela.Import;

/// <summary>
/// VeLA column entry region parser for columns damnatio, specchio, cornice,
/// tipo_di_cornice, campo. This targets <see cref="EpiSupportPart.HasDamnatio"/>,
/// <see cref="EpiSupportPart.HasMirror"/>, <see cref="EpiSupportPart.HasFrame"/>,
/// <see cref="EpiSupportPart.Frame"/>, <see cref="EpiSupportPart.HasField"/>.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.vela.col-supporto-fields")]
public sealed class ColSupportFieldsEntryRegionParser(
    ILogger<ColSupportFieldsEntryRegionParser>? logger = null) : EntryRegionParser,
    IEntryRegionParser
{
    private readonly HashSet<string> _colNames =
    [
        "col-damnatio", "col-specchio", "col-cornice", "col-tipo_di_cornice",
        "col-campo"
    ];
    private readonly ILogger<ColSupportFieldsEntryRegionParser>? _logger = logger;

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
            _logger?.LogError("{Tag} column without any item at region {Region}",
                region.Tag, region);
            throw new InvalidOperationException(
                $"{region.Tag} column without any item at region " + region);
        }

        DecodedTextEntry txt = (DecodedTextEntry)
            set.Entries[region.Range.Start.Entry + 1];
        string? value = VelaHelper.FilterValue(txt.Value, false);
        if (!string.IsNullOrEmpty(value))
        {
            EpiSupportPart part = ctx.EnsurePartForCurrentItem<EpiSupportPart>();

            switch (region.Tag)
            {
                case "col-damnatio":
                    part.HasDamnatio = true;
                    break;
                case "col-specchio":
                    part.HasMirror = true;
                    break;
                case "col-cornice":
                    part.HasFrame = true;
                    break;
                case "col-tipo_di_cornice":
                    part.Frame = value;
                    break;
                case "col-campo":
                    part.HasField = true;
                    break;
            }
        }

        return regionIndex + 1;
    }
}
