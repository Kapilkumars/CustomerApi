using Customer.Metis.Logging.Correlation;
using Customer.Metis.Logging.Enrichers;
using Customer.Metis.SettingsProviders;
using CustomerCustomerApi.Extensions;
using CustomerCustomerFunctions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.CosmosDB;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Enrichers.Span;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Json;
using SumoLogic.Logging.Serilog.Extensions;
using System;
using System.IO;

[assembly: FunctionsStartup(typeof(Startup))]
namespace CustomerCustomerFunctions
{
    public class Startup : IWebJobsStartup
    {
        private string GetEnvironmentName()
        {
            string environmentName = "QA";
            switch (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
            {
                case "Development":
                    environmentName = "Dev";
                    break;
                case "Local":
                    environmentName = "Local";
                    break;
                case "PRODUCTION":
                    environmentName = "Prod";
                    break;
            }
            return environmentName;
        }

        public void Configure(IWebJobsBuilder builder)
        {
            var executionContextOptions = builder.Services.BuildServiceProvider()
                            .GetService<IOptions<ExecutionContextOptions>>().Value;
            //add azure settings
            var jsonSettingsProvider = new JsonSettingsProvider(new[] { Path.Combine(executionContextOptions.AppDirectory,
                                "AppSettings", $"appsettings.{GetEnvironmentName()}.json") }, null, false);
            var _aggregateSettingsProvider = new AggregateSettingsProvider();

            var azureKeyVaultUrl = jsonSettingsProvider.GetSetting("AzureKeyVaultUrl");
            var tenant = jsonSettingsProvider.GetSetting("AZURE_TENANT_ID");
            var appClient = jsonSettingsProvider.GetSetting("AZURE_CLIENT_ID");
            var secret = jsonSettingsProvider.GetSetting("AZURE_CLIENT_SECRET");

            var kvProvider = new AzureKeyVaultSettingsProvider(azureKeyVaultUrl, tenant, appClient, secret);

            _aggregateSettingsProvider.AddProvider(jsonSettingsProvider, 2);
            _aggregateSettingsProvider.AddProvider(kvProvider, 1);

            var connectionString = _aggregateSettingsProvider.GetSetting("CosmosConnection");

            //add connectionString for change feeds
            builder.Services.PostConfigure<CosmosDBOptions>(options =>
            {
                options.ConnectionString = connectionString;
            });

            //add cosmos db repositories
            builder.Services.AddCosmosRepositories(connectionString, "metis-customers");

            //add logging
            var sumoEndPoint = _aggregateSettingsProvider.GetSetting("SumoHttpEndpoint") ?? "";
            var sumoSource = _aggregateSettingsProvider.GetSetting("SumoSourceName");
            var sumoSourceCategory = _aggregateSettingsProvider.GetSetting("SumoSourceCategory");

            builder.Services.AddHttpContextAccessor();

            Log.Logger = new LoggerConfiguration()
                #if DEBUG
                .MinimumLevel.Debug()
                #endif
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                //.ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.BufferedSumoLogic(
                    new Uri(sumoEndPoint),
                    sourceName: sumoSource,
                    formatter: new JsonFormatter(),
                    sourceCategory: sumoSourceCategory)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithSpan()
                .Enrich.CorrelationEnricher()
                .CreateLogger();

            builder.Services.AddLogging(lb =>
            {
                lb.AddSerilog(Log.Logger, true);
            });

            builder.Services.AddCorrelationIdGeneratorService();
        }
    }
}
