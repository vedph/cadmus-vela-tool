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
            "col-numero_righe",
            "col-alfabeto",
            "col-lingua_(iso-639-3)",
            "col-codice_glottologico",
            "col-tipologia_scrittura",
            "col-tipologia_grafica",
            "col-rubricatura",
            "col-maiuscolo\\minuscolo_prevalente",
            "col-sistema_interpuntivo",
            "col-nessi_e_legamenti",
            "col-rigatura",
            "col-abbreviazioni",
            "col-monogrammi",
            "col-lettera_singola",
            "col-lettere_non_interpretabili"
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
            case "col-numero_righe":
                part = ctx.EnsurePartForCurrentItem<GrfWritingPart>();
                part.Counts.Add(new()
                {
                    Id = "row",
                    Value = int.Parse(value, CultureInfo.InvariantCulture)
                });
                break;

            case "col-alfabeto":
                id = VelaHelper.GetThesaurusId(ctx, region,
                    VelaHelper.T_GRF_WRITING_SYSTEMS, value, _logger);
                part = ctx.EnsurePartForCurrentItem<GrfWritingPart>();
                part.System = id;
                break;

            case "col-lingua_(iso-639-3)":
                id = VelaHelper.GetThesaurusId(ctx, region,
                    VelaHelper.T_GRF_WRITING_LANGUAGES, value, _logger);
                part = ctx.EnsurePartForCurrentItem<GrfWritingPart>();
                part.Languages.Add(id);
                break;

            case "col-codice_glottologico":
                id = VelaHelper.GetThesaurusId(ctx, region,
                VelaHelper.T_GRF_WRITING_LANGUAGES, value, _logger);
                part = ctx.EnsurePartForCurrentItem<GrfWritingPart>();

                // if there is no language yet, add it as the first one.
                // Otherwise, append it to the first one because the first
                // in this case is the ISO639-3 code.
                if (part.Languages.Count == 0)
                    part.Languages.Add(id);
                else
                    part.Languages[0] += $"_{id}";
                break;

            case "col-tipologia_scrittura":
                // multiple scripts are separated by comma
                IList<string> tokens = VelaHelper.GetValueList(value, true);
                if (tokens.Count > 0)
                {
                    part = ctx.EnsurePartForCurrentItem<GrfWritingPart>();

                    foreach (string token in tokens)
                    {
                        id = VelaHelper.GetThesaurusId(ctx, region,
                            VelaHelper.T_GRF_WRITING_SCRIPTS, token, _logger);
                        part.Scripts.Add(id);
                    }
                }
                break;

            case "col-tipologia_grafica":
                id = VelaHelper.GetThesaurusId(ctx, region,
                    VelaHelper.T_GRF_WRITING_CASING, value, _logger);
                part = ctx.EnsurePartForCurrentItem<GrfWritingPart>();
                part.Casing = id;
                break;

            case "col-rubricatura":
                if (VelaHelper.GetBooleanValue(value))
                {
                    part = ctx.EnsurePartForCurrentItem<GrfWritingPart>();
                    part.HasRubrics = true;
                }
                break;

            case "col-maiuscolo\\minuscolo_prevalente":
                // TODO ask if this is relevant
                break;

            case "col-sistema_interpuntivo":
                if (VelaHelper.GetBooleanValue(value))
                {
                    part = ctx.EnsurePartForCurrentItem<GrfWritingPart>();
                    part.ScriptFeatures.Add("punctuation");
                }
                break;

            case "col-nessi_e_legamenti":
                if (VelaHelper.GetBooleanValue(value))
                {
                    part = ctx.EnsurePartForCurrentItem<GrfWritingPart>();
                    part.ScriptFeatures.Add("ligature");
                }
                break;

            case "col-rigatura":
                if (VelaHelper.GetBooleanValue(value))
                {
                    part = ctx.EnsurePartForCurrentItem<GrfWritingPart>();
                    part.HasRuling = true;
                }
                break;

            case "col-abbreviazioni":
                if (VelaHelper.GetBooleanValue(value))
                {
                    part = ctx.EnsurePartForCurrentItem<GrfWritingPart>();
                    part.ScriptFeatures.Add("abbreviation");
                }
                break;

            case "col-monogrammi":
                if (VelaHelper.GetBooleanValue(value))
                {
                    part = ctx.EnsurePartForCurrentItem<GrfWritingPart>();
                    part.LetterFeatures.Add("monogram");
                }
                break;

            case "col-lettera_singola":
                if (VelaHelper.GetBooleanValue(value))
                {
                    part = ctx.EnsurePartForCurrentItem<GrfWritingPart>();
                    part.LetterFeatures.Add("letter");
                }
                break;

            case "col-lettere_non_interpretabili":
                if (VelaHelper.GetBooleanValue(value))
                {
                    part = ctx.EnsurePartForCurrentItem<GrfWritingPart>();
                    part.LetterFeatures.Add("unclear");
                }
                break;
        }

        return regionIndex + 1;
    }
}
