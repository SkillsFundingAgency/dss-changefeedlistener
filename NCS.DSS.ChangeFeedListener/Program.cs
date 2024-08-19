using Azure.Core.Serialization;
using DFC.Common.Standard.Logging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NCS.DSS.ChangeFeedListener.ServiceBus;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

var host = new HostBuilder()
.ConfigureFunctionsWorkerDefaults((IFunctionsWorkerApplicationBuilder workerApplication) =>
                {
                    workerApplication.UseNewtonsoftJson();
                })
    .ConfigureServices(services =>
    {
        services.AddSingleton<IServiceBusClient, ServiceBusClient>();
        services.AddSingleton<ILoggerHelper, LoggerHelper>();
        services.AddLogging();
    })
    .Build();
host.Run();


internal static class WorkerConfigurationExtensions
{
    /// <summary>
    /// Calling ConfigureFunctionsWorkerDefaults() configures the Functions Worker to use System.Text.Json for all JSON
    /// serialization and sets JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    /// This method uses DI to modify the JsonSerializerOptions. Call /api/HttpFunction to see the changes.
    /// </summary>
    public static IFunctionsWorkerApplicationBuilder ConfigureSystemTextJson(this IFunctionsWorkerApplicationBuilder builder)
    {
        builder.Services.Configure<JsonSerializerOptions>(jsonSerializerOptions =>
        {
            jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            jsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            jsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;

            // override the default value
            jsonSerializerOptions.PropertyNameCaseInsensitive = false;
        });

        return builder;
    }

    /// <summary>
    /// The functions worker uses the Azure SDK's ObjectSerializer to abstract away all JSON serialization. This allows you to
    /// swap out the default System.Text.Json implementation for the Newtonsoft.Json implementation.
    /// To do so, add the Microsoft.Azure.Core.NewtonsoftJson nuget package and then update the WorkerOptions.Serializer property.
    /// This method updates the Serializer to use Newtonsoft.Json. Call /api/HttpFunction to see the changes.
    /// </summary>
    public static IFunctionsWorkerApplicationBuilder UseNewtonsoftJson(this IFunctionsWorkerApplicationBuilder builder)
    {
        builder.Services.Configure<WorkerOptions>(workerOptions =>
        {
            var settings = NewtonsoftJsonObjectSerializer.CreateJsonSerializerSettings();
            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            settings.NullValueHandling = NullValueHandling.Ignore;

            workerOptions.Serializer = new NewtonsoftJsonObjectSerializer(settings);
        });

        return builder;
    }
}
