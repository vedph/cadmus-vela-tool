using Cadmus.General.Parts;
using Cadmus.Import.Proteus;
using Fusi.Antiquity.Chronology;
using Fusi.Tools;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;

namespace Cadmus.Vela.Import;

/// <summary>
/// VeLA columns data and secolo entry region parser. This targets
/// <see cref="HistoricalDatePart"/>.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.vela.col-data")]
public sealed class ColDatationEntryRegionParser(
    ILogger<ColDatationEntryRegionParser>? logger = null) : EntryRegionParser,
    IEntryRegionParser
{
    private const string COL_DATA = "col-data";
    private const string COL_SECOLO = "col-secolo";
    private readonly ILogger<ColDatationEntryRegionParser>? _logger = logger;

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

        return regions[regionIndex].Tag == COL_DATA ||
               regions[regionIndex].Tag == COL_SECOLO;
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
            _logger?.LogError(
                "{Tag} column without any item at region {Region}",
                region.Tag![4..], region);
            throw new InvalidOperationException(
                $"{region.Tag![4..]} column without any item at region " +
                region);
        }

        DecodedTextEntry txt = (DecodedTextEntry)
            set.Entries[region.Range.Start.Entry + 1];

        if (!string.IsNullOrEmpty(txt.Value))
        {
            HistoricalDatePart part;
            string? value;
            switch (region.Tag)
            {
                // data is a year and comes before secolo and termini ante/post
                case COL_DATA:
                    value = VelaHelper.FilterValue(txt.Value, false);
                    if (!string.IsNullOrEmpty(value))
                    {
                        if (!int.TryParse(value, out int year))
                        {
                            Logger?.LogError(
                                "Invalid year \"{Value}\" at region {Region}",
                                value, region);
                        }
                        else
                        {
                            part = ctx.EnsurePartForCurrentItem<HistoricalDatePart>();
                            part.Date = new HistoricalDate
                            {
                                A = new Datation
                                {
                                    Value = year
                                }
                            };
                        }
                    }
                    break;
                // secolo is a century and comes after data - we assume that
                // it is either data or secolo, not both; if both, data wins.
                // This column can also include a range like XV-XVI.
                case COL_SECOLO:
                    // R secolo
                    value = VelaHelper.FilterValue(txt.Value, true)
                        ?.Replace(" secolo", "").ToUpperInvariant();
                    if (!string.IsNullOrEmpty(value))
                    {
                        // if there is a - it is a range, so split it and
                        // parse each Roman number, creating an interval date
                        if (value.Contains('-'))
                        {
                            string[] parts = value.Split('-');
                            if (parts.Length != 2)
                            {
                                Logger?.LogError(
                                    "Invalid century range \"{Value}\" at region {Region}",
                                    value, region);
                            }
                            int a = RomanNumber.FromRoman(parts[0].Trim());
                            int b = RomanNumber.FromRoman(parts[1].Trim());
                            part = ctx.EnsurePartForCurrentItem<HistoricalDatePart>();
                            part.Date = new HistoricalDate
                            {
                                A = new Datation
                                {
                                    Value = a,
                                    IsCentury = true
                                },
                                B = new Datation
                                {
                                    Value = b,
                                    IsCentury = true
                                }
                            };
                        }
                        else
                        {
                            int n = RomanNumber.FromRoman(value);
                            part = ctx.EnsurePartForCurrentItem<HistoricalDatePart>();
                            if (part.Date is null ||
                                part.Date.GetDateType() == HistoricalDateType.Undefined)
                            {
                                part.Date = new HistoricalDate
                                {
                                    A = new Datation
                                    {
                                        Value = n,
                                        IsCentury = true
                                    }
                                };
                            }
                        }
                    }
                    break;
            }
        }

        return regionIndex + 1;
    }
}
