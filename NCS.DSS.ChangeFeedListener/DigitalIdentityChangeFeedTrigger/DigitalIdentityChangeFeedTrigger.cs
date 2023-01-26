using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.Common.Standard.Logging;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NCS.DSS.ChangeFeedListener.Model;
using NCS.DSS.ChangeFeedListener.ServiceBus;

namespace NCS.DSS.ChangeFeedListener.DigitalIdentityChangeFeedTrigger
{
    public class DigitalIdentityChangeFeedTrigger
    {
        private readonly IServiceBusClient _serviceBusClient;
        private readonly ILoggerHelper _loggerHelper;

        private const string DatabaseName = "%DigitalIdentityDatabaseId%";
        private const string CollectionName = "%DigitalIdentityCollectionId%";
        private const string ConnectionString = "CosmosDBConnectionString";
        private const string LeaseCollectionName = "%DigitalIdentityLeaseCollectionName%";
        private const string LeaseCollectionPrefix = "%DigitalIdentityLeaseCollectionPrefix%";

        public DigitalIdentityChangeFeedTrigger(IServiceBusClient serviceBusClient, ILoggerHelper loggerHelper)
        {
            _serviceBusClient = serviceBusClient;
            _loggerHelper = loggerHelper;
        }

        [FunctionName("DigitalIdentityChangeFeedTrigger")]
        public async Task Run([CosmosDBTrigger(
            DatabaseName,
            CollectionName,
            Connection = ConnectionString,
            LeaseContainerName = LeaseCollectionName,
            LeaseContainerPrefix = LeaseCollectionPrefix,
            CreateLeaseContainerIfNotExists  = true
            )] IReadOnlyList<Document> documents,
            ILogger log)
        {
            try
            {
                foreach (var document in documents)
                {
                    var changeFeedMessageModel = new ChangeFeedMessageModel()
                    {
                        Document = document,
                        IsDigitalIdentity = true
                    };

                    _loggerHelper.LogInformationMessage(log, Guid.NewGuid(), string.Format("Attempting to send digital identity document id: {0} to service bus queue", document.Id));
                    await _serviceBusClient.SendChangeFeedMessageAsync(document, changeFeedMessageModel);
                }
            }
            catch (Exception ex)
            {
                _loggerHelper.LogException(log, Guid.NewGuid(), "Error when trying to send digital identity message to service bus queue", ex);
            }
        }
    }
}
