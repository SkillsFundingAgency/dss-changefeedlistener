﻿using DFC.Common.Standard.Logging;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.ChangeFeedListener.Model;
using NCS.DSS.ChangeFeedListener.ServiceBus;

namespace NCS.DSS.ChangeFeedListener.DigitalIdentityChangeFeedTrigger
{
    public class DigitalIdentityChangeFeedTrigger
    {
        private readonly IServiceBusClient _serviceBusClient;
        private readonly ILoggerHelper _loggerHelper;
        private readonly ILogger _logger;

        private const string DatabaseName = "%DigitalIdentityDatabaseId%";
        private const string CollectionName = "%DigitalIdentityCollectionId%";
        private const string ConnectionString = "CosmosDBConnectionString";
        private const string LeaseCollectionName = "%DigitalIdentityLeaseCollectionName%";
        private const string LeaseCollectionPrefix = "%DigitalIdentityLeaseCollectionPrefix%";

        public DigitalIdentityChangeFeedTrigger(IServiceBusClient serviceBusClient,
            ILoggerHelper loggerHelper,
            ILogger<DigitalIdentityChangeFeedTrigger> logger)
        {
            _serviceBusClient = serviceBusClient;
            _loggerHelper = loggerHelper;
            _logger = logger;
        }

        [Function("DigitalIdentityChangeFeedTrigger")]
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
                        IsDigitalIdentity = true
                    };

                    _loggerHelper.LogInformationMessage(_logger, Guid.NewGuid(), string.Format("Attempting to send digital identity document id: {0} to service bus queue", document.Id));
                    await _serviceBusClient.SendChangeFeedMessageAsync(document, changeFeedMessageModel);
                }
            }
            catch (Exception ex)
            {
                _loggerHelper.LogException(_logger, Guid.NewGuid(), "Error when trying to send digital identity message to service bus queue", ex);
            }
        }
    }
}
