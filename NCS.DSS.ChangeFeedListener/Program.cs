using DFC.Common.Standard.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NCS.DSS.ChangeFeedListener.ServiceBus;

var host = new HostBuilder()
.ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddSingleton<IServiceBusClient, ServiceBusClient>();
        services.AddSingleton<ILoggerHelper, LoggerHelper>();
    })
    .Build();

host.Run();
