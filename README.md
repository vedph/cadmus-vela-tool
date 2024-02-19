# Cadmus VeLA CLI Tool

This is a command-line tool providing administrative functions for [Cadmus VeLA](https://github.com/vedph/cadmus-vela). Currently, it is designed to import data from Excel files into the VeLA database using a [Proteus](https://myrmex.github.io/overview/proteus/)-based pipeline.

To import Excel files (like [this](vela.xlsx)), run the `import` command, passing the path to the JSON pipeline configuration file, e.g.:

```ps1
import c:\users\dfusi\desktop\vela.json
```

Preset profiles can be found under the `Assets` folder of the CLI app.

## History

### 1.0.1

- 2024-02-19: more empty or invalid number checks in parsers.
- 2024-02-10: updated packages and added EOF column option to asset profiles so that import can stop at the first row having column 1 empty. This seems to be required in real-world Excel files, as the last rows are often empty.
- 2024-01-03: updated packages and thesauri.
- 2024-01-31:
  - updated packages.
  - fixes.
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
                region);
            throw new InvalidOperationException(
                "__TAG__ column without any item at region " + region);
        }

        DecodedTextEntry txt = (DecodedTextEntry)
            set.Entries[region.Range.Start.Entry + 1];
        string? value = VelaHelper.FilterValue(txt.Value, false);

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
    _logger?.LogError("Unknown value for tipologia_struttura: \"{value}\" " +
        "at region {region}", value, region);
    id = value;
}

GrfLocalizationPart part =
    ctx.EnsurePartForCurrentItem<GrfLocalizationPart>();
part.ObjectType = id;
```

## Usage

The CLI tool has a unique command used to import data from [Excel documents](https://github.com/vedph/cadmus-vela?tab=readme-ov-file#original-spreadsheet). Syntax:

```ps1
./vela-tool import <JsonProfilePath>
```

where `JsonProfilePath` is the path to the JSON file representing a Proteus import profile. Example:

```ps1
./vela-tool import c:/users/dfusi/desktop/vela.json
```

You can find [preset profiles](./vela-tool/Assets) under the tool's `Assets` folder: these cover different outputs from the same input, namely Markdown dump, Excel dump, and database import.

👉 The typical **import workflow** is:

1. to check for errors, run the [Markdown dump profile](./vela-tool/Assets/Dump-md.json) on your Excel files. Examine the log and ensure that there are no warnings or errors.
2. create a new database by just launching the VeLA API (set seed items count is 0 if required). This will setup facets, flags, thesauri, etc.
3. use the [import profile](./vela-tool/Assets/Import.json) to import Excel files.

>⚠️ If you change any thesaurus in the API seed profile, remember to update also its copy used here by the import tool.

### Dumping

Typically, you will first use the profiles for dumping the import process to ensure that it works properly. There are two dump types: Markdown and Excel. The Markdown dump is the more detailed, and shows the contents of each record read from the source file. The Excel dump is focused on the structure of the input documents, which in this case is less relevant; it shows how the input is decoded into entries, variously grouped into sets and regions. So, while this can be useful to gain a full understanding of the import stages, it is not so relevant in a simple flat input like that provided by Excel files. The Excel dump instead is very relevant when dealing with other source types, like Word documents.

The Markdown dump provides an entry for each row imported, assuming that each row corresponds to an item (a record). For each item using the preset profiles (some dump options are customizable) you get:

- a title, equal to the record's ID.
- the essential metadata linked to the item being imported: ID, description, facet, etc. All these data are calculated automatically, so you do not have to check for them unless you happen to see something really odd. For instance, here is a dump from a mock item:

```txt
- ID: `1f4b2f86-40a6-4618-98be-f2ab5fc91d42`
- description: ``
- facet ID: `graffiti`
- sort key: `castello050001`
- created: `29/01/2024 11:13:06`
- creator: `zeus`
- modified: `29/01/2024 11:13:06`
- user: `zeus`
- flags: `0001`
- parts: 9
```

- the list of all the parts inside the item. For each part, you get a summary line followed by the JSON dump of its contents, like in this example:

- part __2__ / 9: [GrfLocalization] Castello_5 Castello Chiesa di San Martino, religiosa

```json
{
  "place": {
    "pieces": [
      {
        "type": "area",
        "value": "Castello_5"
      },
      {
        "type": "sestiere",
        "value": "Castello"
      },
      {
        "type": "location",
        "value": "Chiesa di San Martino"
      }
    ]
  },
  "function": "religiosa",
  "note": "Chiesa",
  "objectType": "church",
  "indoor": false,
  "id": "39c1681c-5f7b-46c8-abe6-5fd42d01f143",
  "itemId": "1f4b2f86-40a6-4618-98be-f2ab5fc91d42",
  "typeId": "it.vedph.graffiti.localization",
  "timeCreated": "2024-01-29T11:13:06.3371504Z",
  "creatorId": "zeus",
  "timeModified": "2024-01-29T11:13:06.3371504Z",
  "userId": "zeus"
}
```

Here you have part 2 of 9 in this item, of type [GrfLocalization](https://github.com/vedph/cadmus-vela?tab=readme-ov-file#grflocalizationpart).

>Parts are numbered in the order they are found, but this numbering is just an aid for reading the dump document. Any part is just inside the box represented by an item, and has no intrinsic ordering in it.

The parts should include all the data from all the relevant columns of the source record, distributed in the corresponding structures. Each part has a fixed set of metadata and its own model, as dumped by its JSON representation. Please refer to the [VeLA Cadmus model documentation](https://github.com/vedph/cadmus-vela) for more details about items, parts, Excel columns and parsers.

When data read from cells refer to taxonomies ([thesauri](https://myrmex.github.io/overview/cadmus/dev/concepts/thesauri) in Cadmus lingo), the importer maps the text read to the corresponding entry in its target taxonomy. If this is not possible, an error will be logged and the value read as it is will be used.

>All the thesauri defined for VeLA can be read from its configuration profile at [this page](https://github.com/vedph/cadmus-vela-api/blob/master/CadmusVelaApi/wwwroot/seed-profile.json).

For instance, consider the thesaurus for scripts in its JSON form:

```json
{
    "id": "grf-writing-scripts@en",
    "entries": [
    {
        "id": "-",
        "value": "---"
    },
    {
        "id": "gothic",
        "value": "gotica"
    },
    {
        "id": "chancery",
        "value": "cancelleresca"
    }
    ]
}
```

Here we have 3 entries. Entries are identified by their `id` property, which has a corresponding human-friendly value in the `value` property. Internally the database stores the ID, even if the UI displays the corresponding value (here using Italian). As Excel files were created manually, their cells contain values rather than identifiers. So, for instance you find a cell with text `gotica`. The importer knows that the column including that cell refers to the thesaurus identified by `grf-writing-scripts@en`, so that it looks up its entries by value; in this example it finds that the corresponding ID is `gothic`. This is the value stored in the part, and thus in the database.

If for some reason (e.g. a typing error, or a missing entry) the example cell contained a value which is not found among the thesaurus entries, like `mercantile`, a mapping error will be logged, and the cell's value as it is will be stored in the part. This is just a fallback mechanism to avoid data loss and help users identify issues; the action to take here is either fixing a type error in the Excel file, or adding a new entry to the thesaurus.

So, dumping or importing a file without critical errors does not imply that the process is error-free. You should always look at the dump on one side, and at the log files on the other side, fix all the issues, and repeat until no more errors are found.

### Examining Log

When executed, the tool also produces a log file. You can find the log under the tool's folder, named like `vela-logYYYMMDD.txt` where `YYYYMMDD` is the log creation date.

The log is a usual log file, where each entry is a line and has a category:

- INF = information: this is just to help you read the log.
- WRN = warning: this is the indication of a potential issue. You should check this.
- ERR = error: this is the indication of an import error. There should be no ERR left in the log for the import process to be considered valid.

For instance, here is an excerpt from the log for the dump of the mock document:

```txt
2024-01-29 12:13:06.286 +01:00 [INF] --- Reading set #1
2024-01-29 12:13:06.319 +01:00 [INF] -- ROW: 2
2024-01-29 12:13:06.338 +01:00 [ERR] Unknown value for funzione_attuale: religiosa
2024-01-29 12:13:06.340 +01:00 [ERR] Unknown value for tipologia_struttura: muro at region 29.0-31.0=col-supporto
2024-01-29 12:13:06.355 +01:00 [ERR] Unknown value for col-tipologia_grafica: Maiuscolo at region 89.0-91.0=col-tipologia_grafica
```

As you can see, here the log provides 3 errors related to missing entries in the target thesauri. For each of these errors you have the indication of its type (e.g. "Unknown value for..."), the column being handled (e.g. "funzione_attuale"), and the text value being imported (e.g. "religiosa"). Armed with this knowledge, fixing issues by supplying new values into the corresponding thesauri will be a trivial operation.
