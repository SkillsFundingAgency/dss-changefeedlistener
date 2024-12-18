using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.ChangeFeedListener.Model;
using NCS.DSS.ChangeFeedListener.ServiceBus;
using System.Text.Json;

namespace NCS.DSS.ChangeFeedListener.ContactChangeFeedTrigger
{
    public class ContactChangeFeedTrigger
    {
        private readonly IChangeFeedListenerServiceBusClient _serviceBusClient;
        private readonly ILogger _logger;

        private const string DatabaseName = "%ContactDatabaseId%";
        private const string CollectionName = "%ContactCollectionId%";
        private const string ConnectionString = "CosmosDBConnectionString";
        private const string LeaseCollectionName = "%ContactLeaseCollectionName%";
        private const string LeaseCollectionPrefix = "%ContactLeaseCollectionPrefix%";

        public ContactChangeFeedTrigger(IChangeFeedListenerServiceBusClient serviceBusClient,
            ILogger<ContactChangeFeedTrigger> logger)
        {
            _serviceBusClient = serviceBusClient;            
            _logger = logger;
        }

        [Function("ContactChangeFeedTrigger")]
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
                        IsContact = true
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