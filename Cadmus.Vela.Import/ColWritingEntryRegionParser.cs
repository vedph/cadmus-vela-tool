using Cadmus.Import.Proteus;
using Cadmus.Vela.Parts;
using Fusi.Tools.Configuration;
using Microsoft.Extensions.Logging;
using Proteus.Core.Entries;
using Proteus.Core.Regions;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

namespace Cadmus.Vela.Import;

/// <summary>
/// VeLA entry region parser for columns numero_righe, alfabeto,
/// lingua_(iso-639-3), codice_glottologico, tipologia_scrittura,
/// tipologia_grafica, rubricatura, maiuscolo\minuscolo_prevalente,
/// sistema_interpuntivo, nessi_e_legamenti, rigatura, abbreviazioni,
/// monogrammi, lettera_singola, lettere_non_interpretabili. This targets
/// <see cref="GrfWritingPart"/>.
/// </summary>
/// <seealso cref="EntryRegionParser" />
/// <seealso cref="IEntryRegionParser" />
[Tag("entry-region-parser.vela.col-writing")]
public sealed class ColWritingEntryRegionParser : EntryRegionParser,
    IEntryRegionParser
{
    private readonly ILogger<ColWritingEntryRegionParser>? _logger;
    private readonly HashSet<string> _tags =
        [
            "numero_righe",
            "alfabeto",
            "lingua_(iso-639-3)",
            "codice_glottologico",
            "tipologia_scrittura",
            "tipologia_grafica",
            "rubricatura",
            "maiuscolo\\minuscolo_prevalente",
            "sistema_interpuntivo",
            "nessi_e_legamenti",
            "rigatura",
            "abbreviazioni",
            "monogrammi",
            "lettera_singola",
            "lettere_non_interpretabili"
        ];

    /// <summary>
    /// Initializes a new instance of the <see cref="ColWritingEntryRegionParser"/>
    /// class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public ColWritingEntryRegionParser(
        ILogger<ColWritingEntryRegionParser>? logger = null)
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

        return _tags.Contains(regions[regionIndex].Tag ?? "");
    }

    private string? GetThesaurusId(CadmusEntrySetContext ctx, EntryRegion region,
        string thesaurusId, string? value)
    {
        if (string.IsNullOrEmpty(value)) return null;

        string? id = ctx.ThesaurusEntryMap!.GetEntryId(thesaurusId, value);

        if (id == null)
        {
            _logger?.LogError("Unknown value for {tag}: {value} " +
                "at region {region}", region.Tag, value, region);
            id = value;
        }

        return id;
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

        if (string.IsNullOrEmpty(value)) return regionIndex + 1;

        GrfWritingPart? part = null;
        string? id;

        switch (region.Tag)
        {
            case "numero_righe":
                part = ctx.EnsurePartForCurrentItem<GrfWritingPart>();
                part.Counts.Add(new()
                {
                    Id = "row",
                    Value = int.Parse(value, CultureInfo.InvariantCulture)
                });
                break;

            case "alfabeto":
                id = GetThesaurusId(ctx, region,
                    VelaHelper.T_GRF_WRITING_SYSTEMS, value);
                if (id != null)
                {
                    part = ctx.EnsurePartForCurrentItem<GrfWritingPart>();
                    part.System = GetThesaurusId(ctx, region,
                        VelaHelper.T_GRF_WRITING_SYSTEMS, value);
                }
                break;

            case "lingua_(iso-639-3)":
                id = GetThesaurusId(ctx, region,
                    VelaHelper.T_GRF_WRITING_LANGUAGES, value);
                if (id != null)
                {
                    part = ctx.EnsurePartForCurrentItem<GrfWritingPart>();
                    part.Languages.Add(id);
                }
                break;

            case "codice_glottologico":
                id = GetThesaurusId(ctx, region,
                    VelaHelper.T_GRF_WRITING_LANGUAGES, value);
                if (id != null)
                {
                    part = ctx.EnsurePartForCurrentItem<GrfWritingPart>();

                    // if there is no language yet, add it as the first one.
                    // Otherwise, append it to the first one because the first
                    // in this case is the ISO639-3 code.
                    if (part.Languages.Count == 0)
                        part.Languages.Add(id);
                    else
                        part.Languages[0] += $"_{id}";
                }
                break;

            case "tipologia_scrittura":
                // multiple scripts are separated by comma
                IList<string> ids = (
                    from v in VelaHelper.GetValueList(value, true)
                    let vi = GetThesaurusId(ctx, region,
                        VelaHelper.T_GRF_WRITING_SCRIPTS, v)
                    where vi != null
                    select vi).ToList();

                if (ids.Count > 0)
                {
                    part = ctx.EnsurePartForCurrentItem<GrfWritingPart>();
                    part.Scripts.AddRange(ids);
                }
                break;

            case "tipologia_grafica":
                id = GetThesaurusId(ctx, region,
                    VelaHelper.T_GRF_WRITING_CASING, value);
                if (id != null)
                {
                    part = ctx.EnsurePartForCurrentItem<GrfWritingPart>();
                    part.Casing = GetThesaurusId(ctx, region,
                        VelaHelper.T_GRF_WRITING_CASING, value);
                }
                break;

            case "rubricatura":
                if (VelaHelper.GetBooleanValue(value))
                {
                    part = ctx.EnsurePartForCurrentItem<GrfWritingPart>();
                    part.HasRubrics = true;
                }
                break;

            case "maiuscolo\\minuscolo_prevalente":
                // TODO ask if this is relevant
                break;

            case "sistema_interpuntivo":
                if (VelaHelper.GetBooleanValue(value))
                {
                    part = ctx.EnsurePartForCurrentItem<GrfWritingPart>();
                    part.ScriptFeatures.Add("punctuation");
                }
                break;

            case "nessi_e_legamenti":
                if (VelaHelper.GetBooleanValue(value))
                {
                    part = ctx.EnsurePartForCurrentItem<GrfWritingPart>();
                    part.ScriptFeatures.Add("ligature");
                }
                break;

            case "rigatura":
                if (VelaHelper.GetBooleanValue(value))
                {
                    part = ctx.EnsurePartForCurrentItem<GrfWritingPart>();
                    part.HasRuling = true;
                }
                break;

            case "abbreviazioni":
                if (VelaHelper.GetBooleanValue(value))
                {
                    part = ctx.EnsurePartForCurrentItem<GrfWritingPart>();
                    part.ScriptFeatures.Add("abbreviation");
                }
                break;

            case "monogrammi":
                break;
            case "lettera_singola":
                break;
            case "lettere_non_interpretabili":
                break;
        }

        return regionIndex + 1;
    }
}
