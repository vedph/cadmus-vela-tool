# Cadmus VeLA CLI Tool

This is a command-line tool providing administrative functions for [Cadmus VeLA](https://github.com/vedph/cadmus-vela). Currently, it is designed to import data from Excel files into the VeLA database using a [Proteus](https://myrmex.github.io/overview/proteus/)-based pipeline.

To import Excel files, run the `import` command, passing the path to the JSON pipeline configuration file, e.g.:

```ps1
import c:\users\dfusi\desktop\vela.json
```

Preset profiles can be found under the `Assets` folder of the CLI app.

## History

- 2024-01-26: updated packages to allow fallback column numbering in case of empty column names.
- 2024-01-25: updated packages.
- 2024-01-19: updated packages and profiles.
- 2024-01-18:
  - updated packages.
  - added column name filtering to asset profiles, as our source has column names with whitespaces and various casing.

## Template

Template for region parser:

- `__TAG__`: the region tag.
- `__NAME__` the class name.

```cs
using Cadmus.Import.Proteus;
using Cadmus.Refs.Bricks;
using Cadmus.Vela.Parts;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Collections.Generic;

namespace Cadmus.Vela.Import;

/// <summary>
/// VeLA column __TAG__ entry region parser. This targets TODO.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.vela.col-__TAG__")]
public sealed class Col__NAME__EntryRegionParser : EntryRegionParser,
    IEntryRegionParser
{
    private readonly ILogger<Col__NAME__EntryRegionParser>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="Col__NAME__EntryRegionParser"/>
    /// class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public Col__NAME__EntryRegionParser(
        ILogger<Col__NAME__EntryRegionParser>? logger = null)
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

        return regions[regionIndex].Tag == "col-__TAG__";
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
            _logger?.LogError("__TAG__ column without any item at region {region}",
                regions[regionIndex]);
            throw new InvalidOperationException(
                "__TAG__ column without any item at region " + regions[regionIndex]);
        }

        DecodedTextEntry txt = (DecodedTextEntry)
            set.Entries[region.Range.Start.Entry + 1];
        string? __TAG__ = VelaHelper.FilterValue(txt.Value);

        // TODO

        return regionIndex + 1;
    }
}
```

Variant for thesaurus entry value:

```cs
string? value = VelaHelper.FilterValue(txt.Value);
string? id = value != null
    ? ctx.ThesaurusEntryMap!.GetEntryId(
        VelaHelper.T_SUPPORT_OBJECT_TYPES, value)
    : null;

if (id == null)
{
    _logger?.LogError("Unknown value for tipologia_struttura: {value} " +
        "at region {region}", value, regions[regionIndex]);
    id = value;
}

GrfLocalizationPart part =
    ctx.EnsurePartForCurrentItem<GrfLocalizationPart>();
part.ObjectType = id;
```
