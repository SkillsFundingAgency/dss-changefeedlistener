using DFC.Common.Standard.Logging;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NCS.DSS.ChangeFeedListener;
using NCS.DSS.ChangeFeedListener.ServiceBus;

[assembly: FunctionsStartup(typeof(Startup))]
namespace NCS.DSS.ChangeFeedListener
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<IServiceBusClient, ServiceBusClient>();
            builder.Services.AddSingleton<ILoggerHelper, LoggerHelper>();
        }
    }
}