using Cadmus.Core;
using Cadmus.Import.Proteus;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Cadmus.Vela.Import;

/// <summary>
/// VeLA row entry region parser. This resets the context and adds a new item
/// to it.
/// <para>Tag: <c>entry-region-parser.vela.row</c>.</para>
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
/// <remarks>
/// Initializes a new instance of the <see cref="RowEntryRegionParser"/>
/// class.
/// </remarks>
/// <param name="logger">The logger.</param>
[Tag("entry-region-parser.vela.row")]
public sealed class RowEntryRegionParser(
    ILogger<RowEntryRegionParser>? logger = null) :
    EntryRegionParser, IEntryRegionParser
{
    private readonly ILogger<RowEntryRegionParser>? _logger = logger;

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

        return regions[regionIndex].Tag == "row";
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

        set.Context.Reset();

        // find the first row-start command
        DecodedCommandEntry? row = null;
        EntryRegion region = regions[regionIndex];
        for (int i = region.Range.Start.Entry; i <= region.Range.End.Entry; i++)
        {
            if (set.Entries[i] is DecodedCommandEntry cmd &&
                cmd.Name == "row-start")
            {
                row = cmd;
                break;
            }
        }
        if (row == null)
        {
            _logger?.LogError("Row command not found in region {Region}",
                region);
            throw new InvalidOperationException(
                "Row command not found in region " + region);
        }

        // log row's Y
        int y = int.Parse(row.GetArgument("y")!, CultureInfo.InvariantCulture);
        _logger?.LogInformation("-- ROW: {Row}", y);

        // add item for the row
        Item item = new()
        {
            FacetId = "graffiti",
            CreatorId = "zeus",
            UserId = "zeus",
            Flags = VelaHelper.F_IMPORTED
        };
        CadmusEntrySetContext ctx = (CadmusEntrySetContext)set.Context;
        ctx.Items.Clear();
        ctx.Items.Add(item);

        return regionIndex + 1;
    }
}
