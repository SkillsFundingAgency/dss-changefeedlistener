using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.Common.Standard.Logging;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Logging;
using NCS.DSS.ChangeFeedListener.Model;
using NCS.DSS.ChangeFeedListener.ServiceBus;
using Microsoft.Azure.Functions.Worker;

namespace NCS.DSS.ChangeFeedListener.WebChatChangeFeedTrigger
{
    public class WebChatChangeFeedTrigger
    {
        private readonly IServiceBusClient _serviceBusClient;
        private readonly ILoggerHelper _loggerHelper;
        private readonly ILogger _logger;

        private const string DatabaseName = "%WebChatDatabaseId%";
        private const string CollectionName = "%WebChatCollectionId%";
        private const string ConnectionString = "CosmosDBConnectionString";
        private const string LeaseCollectionName = "%WebChatLeaseCollectionName%";
        private const string LeaseCollectionPrefix = "%WebChatLeaseCollectionPrefix%";

        public WebChatChangeFeedTrigger(IServiceBusClient serviceBusClient, ILoggerHelper loggerHelper, ILogger logger)
        {
            _serviceBusClient = serviceBusClient;
            _loggerHelper = loggerHelper;
            _logger = logger;
        }

        [Function("WebChatChangeFeedTrigger")]
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
                        IsWebChat = true
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