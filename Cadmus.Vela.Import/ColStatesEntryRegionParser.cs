﻿using Cadmus.Import.Proteus;
using Cadmus.General.Parts;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using Cadmus.Mat.Bricks;

namespace Cadmus.Vela.Import;

/// <summary>
/// VeLA column states entry region parser. This targets
/// <see cref="PhysicalStatesPart"/>.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.vela.col-states")]
public sealed class ColStatesEntryRegionParser(
    ILogger<ColStatesEntryRegionParser>? logger = null) : EntryRegionParser,
    IEntryRegionParser
{
    private const string COL_STATE = "col-stato_di_conservazione";
    private const string COL_DATE1 = "col-data_prima_ricognizione";
    private const string COL_DATE2 = "col-data_ultima_ricognizione";

    private readonly ILogger<ColStatesEntryRegionParser>? _logger = logger;

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

        return regions[regionIndex].Tag == COL_STATE ||
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
            _logger?.LogError("{Tag} column without any item at region {Region}",
                region.Tag, region);
            throw new InvalidOperationException(
                $"{region.Tag} column without any item at region {region}");
        }

        DecodedTextEntry txt = (DecodedTextEntry)
            set.Entries[region.Range.Start.Entry + 1];
        string? value = VelaHelper.FilterValue(txt.Value, false);

        if (!string.IsNullOrEmpty(value))
        {
            PhysicalStatesPart part =
                ctx.EnsurePartForCurrentItem<PhysicalStatesPart>();

            // target part's last state
            PhysicalState? state = part.States.LastOrDefault();
            if (state == null)
            {
                state = new()
                {
                    Type = "-"
                };
                part.States.Add(state);
            }

            DateOnly? dt;
            switch (region.Tag)
            {
                case COL_STATE:
                    string? type = VelaHelper.FilterValue(txt.Value, true);
                    state.Type = string.IsNullOrEmpty(type)
                        ? "-"
                        : VelaHelper.GetThesaurusId(ctx, region,
                            VelaHelper.T_PHYSICAL_STATES, type, _logger);
                    break;

                case COL_DATE1:
                    dt = VelaHelper.GetDateValue(value);
                    if (dt == null)
                    {
                        _logger?.LogError("{Tag} column with invalid date value " +
                            "{Value} at region {Region}", region.Tag, value, region);
                    }
                    else
                    {
                        state.Date = dt.Value;
                    }
                    break;

                case COL_DATE2:
                    // date2 is a new state after the first one: inherit the type
                    state = new()
                    {
                        Type = part.States.Count > 0
                            ? part.States[0].Type : "-"
                    };
                    part.States.Add(state);

                    dt = VelaHelper.GetDateValue(value);
                    if (dt == null)
                    {
                        _logger?.LogError("{Tag} column with invalid date value " +
                            "{Value} at region {Region}", region.Tag, value, region);
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
