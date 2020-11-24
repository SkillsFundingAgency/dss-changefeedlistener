using DFC.Common.Standard.Logging;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NCS.DSS.ChangeFeedListener.Ioc;
using NCS.DSS.ChangeFeedListener.ServiceBus;

[assembly: WebJobsStartup(typeof(WebJobsExtensionStartup), "Web Jobs Extension Startup")]

namespace NCS.DSS.ChangeFeedListener.Ioc
{
    public class WebJobsExtensionStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            ConfigureFunctionServices(builder);
        }

        private void ConfigureFunctionServices(IWebJobsBuilder builder)
        {
            builder.Services.AddSingleton<IServiceBusClient, ServiceBusClient>();
            builder.Services.AddSingleton<ILoggerHelper, LoggerHelper>();
        }
    }
}