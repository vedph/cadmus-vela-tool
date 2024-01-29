using Cadmus.Import.Proteus;
using Cadmus.Vela.Parts;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cadmus.Vela.Import;

/// <summary>
/// VeLA column states entry region parser. This targets <see cref="GrfStatesPart"/>.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.vela.col-states")]
public sealed class ColStatesEntryRegionParser : EntryRegionParser,
    IEntryRegionParser
{
    private const string COL_NOTE = "osservazioni_sullo_stato_di_conservazione";
    private const string COL_DATE1 = "data_primo_rilievo";
    private const string COL_DATE2 = "data_ultima_ricognizione";

    private readonly ILogger<ColStatesEntryRegionParser>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ColStatesEntryRegionParser"/>
    /// class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public ColStatesEntryRegionParser(
        ILogger<ColStatesEntryRegionParser>? logger = null)
    {
        _logger = logger;
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

        return regions[regionIndex].Tag == COL_NOTE ||
               regions[regionIndex].Tag == COL_DATE1 ||
               regions[regionIndex].Tag == COL_DATE2;
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
        string? value = VelaHelper.FilterValue(txt.Value, false);

        if (!string.IsNullOrEmpty(value))
        {
            GrfStatesPart part = ctx.EnsurePartForCurrentItem<GrfStatesPart>();

            GrfState? state = part.States.LastOrDefault();
            if (state == null)
            {
                state = new();
                part.States.Add(state);
            }

            DateTime? dt;
            switch (region.Tag)
            {
                case COL_NOTE:
                    state.Note = value;
                    break;

                case COL_DATE1:
                    dt = VelaHelper.GetDateValue(value);
                    if (dt == null)
                    {
                        _logger?.LogError("{tag} column with invalid date value " +
                            "{value} at region {region}", region.Tag, value, region);
                    }
                    else
                    {
                        state.Date = dt.Value;
                    }
                    break;

                case COL_DATE2:
                    // date2 is a new state after the first one
                    state = new();
                    part.States.Add(state);

                    dt = VelaHelper.GetDateValue(value);
                    if (dt == null)
                    {
                        _logger?.LogError("{tag} column with invalid date value " +
                            "{value} at region {region}", region.Tag, value, region);
                    }
                    else
                    {
                        state.Date = dt.Value;
                    }
                    break;
            }
        }

        return regionIndex + 1;
    }
}
