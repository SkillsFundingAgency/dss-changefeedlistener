using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.ChangeFeedListener.Model;
using NCS.DSS.ChangeFeedListener.ServiceBus;
using System.Text.Json;

namespace NCS.DSS.ChangeFeedListener.CollectionChangeFeedTrigger
{
    public class CollectionChangeFeedTrigger
    {
        private readonly IChangeFeedListenerServiceBusClient _serviceBusClient;
        private readonly ILogger<CollectionChangeFeedTrigger> _logger;

        private const string DatabaseName = "%DataCollectionsDatabaseId%";
        private const string CollectionName = "%DataCollectionsCollectionId%";
        private const string ConnectionString = "CosmosDBConnectionString";
        private const string LeaseCollectionName = "%DataCollectionsLeaseCollectionName%";
        private const string LeaseCollectionPrefix = "%DataCollectionsLeaseCollectionPrefix%";

        public CollectionChangeFeedTrigger(IChangeFeedListenerServiceBusClient serviceBusClient,
            ILogger<CollectionChangeFeedTrigger> logger)
        {
            _serviceBusClient = serviceBusClient;
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
            )] IReadOnlyList<JsonDocument> documents)
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

                    var documentId = document.RootElement.GetProperty("id").ToString();
                    _logger.LogInformation("Attempting to send document id: {DocumentID} to service bus queue", documentId);
                    await _serviceBusClient.SendChangeFeedMessageAsync(documentId, changeFeedMessageModel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when trying to send message to service bus queue");
            }
        }
    }
}