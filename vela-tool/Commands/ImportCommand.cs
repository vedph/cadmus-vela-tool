using Cadmus.Import.Proteus;
using Cadmus.Vela.Cli.Services;
using Microsoft.Extensions.Logging;
using Proteus.Core.Regions;
using Proteus.Entries.Config;
using Proteus.Entries.Pipeline;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Cadmus.Vela.Cli.Commands;

internal sealed class ImportCommand : AsyncCommand<ImportCommandSettings>
{
    private static void ShowSettings(ImportCommandSettings settings)
    {
        AnsiConsole.MarkupLine("[yellow]RUN IMPORT PIPELINE[/]");
        AnsiConsole.MarkupLine($"Pipeline: [cyan]{settings.PipelinePath}[/]");
    }

    private static string LoadFileContent(string path)
    {
        using var reader = new StreamReader(path);
        return reader.ReadToEnd();
    }

    private ThesaurusEntryMap LoadThesaurusMap()
    {
        string path = Path.Combine(
            Directory.GetCurrentDirectory(), "Assets", "Thesauri.json");

        ThesaurusEntryMap map = new();
        using Stream stream = File.OpenText(path).BaseStream;
        map.Load(stream);
        return map;
    }

    public override async Task<int> ExecuteAsync(CommandContext context,
        ImportCommandSettings settings)
    {
        ShowSettings(settings);

        int count = 0;
        await AnsiConsole.Status().StartAsync("Running pipeline...", async ctx =>
        {
            ctx.Spinner(Spinner.Known.Ascii);

            // load pipeline config
            ctx.Status("Building pipeline...");
            string config = LoadFileContent(settings.PipelinePath!);

            // build pipeline (either from stock components or from plugin)
            IEntryPipelineFactory factory = PipelineFactoryProvider.GetFactory(config);
            EntryPipeline pipeline;
            try
            {
                pipeline = new(factory);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                throw;
            }

            // get entry reader
            ctx.Status("Getting entries set reader...");
            EntrySetReader setReader = factory.GetEntrySetReader();

            // open exporters outputs
            if (pipeline.Exporters.Count > 0)
            {
                ctx.Status("Opening exporters...");
                foreach (IEntrySetExporter exporter in pipeline.Exporters)
                {
                    await exporter.OpenAsync();
                }
            }

            // process sets
            ThesaurusEntryMap map = LoadThesaurusMap();
            ctx.Status("Reading entry sets...");
            pipeline.Start();
            try
            {
                while (setReader.Read())
                {
                    CadmusEntrySetContext c =
                        (CadmusEntrySetContext)setReader.Set.Context;
                    c.ThesaurusEntryMap = map;

                    count++;
                    if (count % 10 == 0)
                    {
                        ctx.Status($"Reading entry sets... {count}");
                    }
                    factory.Logger?.LogInformation("--- Reading set #{Number}",
                        setReader.Set.Context.Number);
                    await pipeline.ExecuteAsync(setReader.Set);
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
                throw;
            }
            finally
            {
                AnsiConsole.WriteLine();
                pipeline.End();
            }

            // close exporters output
            if (pipeline.Exporters.Count > 0)
            {
                ctx.Status("Closing exporters...");
                foreach (IEntrySetExporter exporter in pipeline.Exporters)
                {
                    await exporter.CloseAsync();
                }
            }
        });
        AnsiConsole.WriteLine($"\nSets read: {count}");

        return 0;
    }
}

public class ImportCommandSettings : CommandSettings
{
    [CommandArgument(0, "<PIPELINE_PATH>")]
    [Description("The pipeline configuration file path")]
    public string PipelinePath { get; set; } = "";
}
