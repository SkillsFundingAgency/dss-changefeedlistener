using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.ChangeFeedListener.Model;
using NCS.DSS.ChangeFeedListener.ServiceBus;
using System.Text.Json;

namespace NCS.DSS.ChangeFeedListener.AddressChangeFeedTrigger
{
    public class AddressChangeFeedTrigger
    {
        private readonly IChangeFeedListenerServiceBusClient _serviceBusClient;
        private readonly ILogger<AddressChangeFeedTrigger> _logger;

        private const string DatabaseName = "%AddressDatabaseId%";
        private const string CollectionName = "%AddressCollectionId%";
        private const string ConnectionString = "CosmosDBConnectionString";
        private const string LeaseCollectionName = "%AddressLeaseCollectionName%";
        private const string LeaseCollectionPrefix = "%AddressLeaseCollectionPrefix%";

        public AddressChangeFeedTrigger(IChangeFeedListenerServiceBusClient serviceBusClient,
            ILogger<AddressChangeFeedTrigger> logger)
        {
            _serviceBusClient = serviceBusClient;
            _logger = logger;
        }

        [Function("AddressChangeFeedTrigger")]
        public async Task Run([CosmosDBTrigger(
            DatabaseName,
            CollectionName,
            Connection = ConnectionString,
            LeaseContainerName = LeaseCollectionName,
            LeaseContainerPrefix = LeaseCollectionPrefix,
            CreateLeaseContainerIfNotExists  = true
            )]  IReadOnlyList<JsonDocument> documents)
        {
            try
            {
                foreach (var document in documents)
                {
                    var changeFeedMessageModel = new ChangeFeedMessageModel()
                    {
                        Document = document,
                        IsAddress = true
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