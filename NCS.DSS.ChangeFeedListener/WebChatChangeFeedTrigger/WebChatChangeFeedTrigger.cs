using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.ChangeFeedListener.Model;
using NCS.DSS.ChangeFeedListener.ServiceBus;

namespace NCS.DSS.ChangeFeedListener.WebChatChangeFeedTrigger
{
    public class WebChatChangeFeedTrigger
    {
        private readonly IChangeFeedListenerServiceBusClient _serviceBusClient;
       
        private readonly ILogger _logger;

        private const string DatabaseName = "%WebChatDatabaseId%";
        private const string CollectionName = "%WebChatCollectionId%";
        private const string ConnectionString = "CosmosDBConnectionString";
        private const string LeaseCollectionName = "%WebChatLeaseCollectionName%";
        private const string LeaseCollectionPrefix = "%WebChatLeaseCollectionPrefix%";

        public WebChatChangeFeedTrigger(IChangeFeedListenerServiceBusClient serviceBusClient,
            
            ILogger<WebChatChangeFeedTrigger> logger)
        {
            _serviceBusClient = serviceBusClient;
            
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
            )] IReadOnlyList<JsonDocument> documents)
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