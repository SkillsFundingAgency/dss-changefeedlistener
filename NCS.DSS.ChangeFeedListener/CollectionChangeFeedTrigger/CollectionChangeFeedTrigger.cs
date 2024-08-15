using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.Common.Standard.Logging;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Logging;
using NCS.DSS.ChangeFeedListener.Model;
using NCS.DSS.ChangeFeedListener.ServiceBus;
using Microsoft.Azure.Functions.Worker;

namespace NCS.DSS.ChangeFeedListener.CollectionChangeFeedTrigger
{
    public class CollectionChangeFeedTrigger
    {
        private readonly IServiceBusClient _serviceBusClient;
        private readonly ILoggerHelper _loggerHelper;
        private readonly ILogger _logger;

        private const string DatabaseName = "%DataCollectionsDatabaseId%";
        private const string CollectionName = "%DataCollectionsCollectionId%";
        private const string ConnectionString = "CosmosDBConnectionString";
        private const string LeaseCollectionName = "%DataCollectionsLeaseCollectionName%";
        private const string LeaseCollectionPrefix = "%DataCollectionsLeaseCollectionPrefix%";

        public CollectionChangeFeedTrigger(IServiceBusClient serviceBusClient, ILoggerHelper loggerHelper, ILogger logger)
        {
            _serviceBusClient = serviceBusClient;
            _loggerHelper = loggerHelper;
            _logger = logger;
        }

        [Function("CollectionChangeFeedTrigger")]
        public async Task Run([CosmosDBTrigger(
            DatabaseName,
            CollectionName,
            Connection = ConnectionString,
            LeaseContainerName = LeaseCollectionName,
            LeaseContainerPrefix = LeaseCollectionPrefix,
            CreateLeaseContainerIfNotExists  = true
            )] IReadOnlyList<Document> documents)
        {
            try
            {
                foreach (var document in documents)
                {
                    var changeFeedMessageModel = new ChangeFeedMessageModel()
                    {
                        Document = document,
                        IsCollection = true
                    };

                    _loggerHelper.LogInformationMessage(_logger, Guid.NewGuid(), string.Format("Attempting to send document id: {0} to service bus queue", document.Id));
                    await _serviceBusClient.SendChangeFeedMessageAsync(document, changeFeedMessageModel);
                }
            }
            catch (Exception ex)
            {
                _loggerHelper.LogException(_logger, Guid.NewGuid(), "Error when trying to send message to service bus queue", ex);
            }
        }
    }
}