using Cadmus.Import.Proteus;
using Fusi.Microsoft.Extensions.Configuration.InMemoryJson;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Proteus.Entries.Config;
using Proteus.Entries.Regions;
using Proteus.Extras;
using Serilog;
using System;
using System.IO;
using System.Reflection;

namespace Cadmus.Vela.Cli.Services;

internal static class PipelineFactoryProvider
{
    private static IHost GetHost(string config)
    {
        string logFilePath = Path.Combine(
              Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "",
              "vela-log.txt");

        return Host.CreateDefaultBuilder().ConfigureServices((_, services) =>
        {
            // logger for DI
            services.AddSingleton(sp =>
            {
                var factory = sp.GetRequiredService<ILoggerFactory>();
                return factory.CreateLogger("Logger");
            });

            // text filter
            // services.AddSingleton<ITextFilter, StandardTextFilter>();

            EntryPipelineFactory.ConfigureServices(services,
                // Proteus.Entries
                typeof(ExcelDumpEntryRegionParser).Assembly,
                // Proteus.Extras
                typeof(ExcelEntryReader).Assembly,
                // Cadmus.Import.Proteus
                typeof(CadmusEntrySetContext).Assembly);
            // TODO Proteus.Vela
            // typeof(MnPatternEntryRegionParser).Assembly,
        })
            .ConfigureLogging(logging =>
            {
                // clear all providers: this avoids the default console logger
                logging.ClearProviders();
            })
            .UseSerilog((hostingContext, services, loggerConfiguration)
                => loggerConfiguration
                // .ReadFrom.Configuration(hostingContext.Configuration)
                .Enrich.FromLogContext()
                .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day),
                    writeToProviders: true)
            // extension method from Fusi library
            .AddInMemoryJson(config)
            .Build();
    }

    public static IEntryPipelineFactory GetFactory(string config)
    {
        ArgumentNullException.ThrowIfNull(config);

        return new EntryPipelineFactory(GetHost(config))
        {
            ConnectionString = ConfigurationService.Configuration
                .GetConnectionString("Default")
        };
    }
}
