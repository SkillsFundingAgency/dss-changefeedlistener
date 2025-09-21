using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.ChangeFeedListener.Constants;
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
            Connection = ConfigKeys.CosmosDBConnectionPrefix,
            LeaseContainerName = LeaseCollectionName,
            LeaseContainerPrefix = LeaseCollectionPrefix,
            CreateLeaseContainerIfNotExists  = true
            )]  IReadOnlyList<JsonDocument> documents)
        {
            var functionName = nameof(AddressChangeFeedTrigger);
            _logger.LogTrace("Function {FunctionName} has been invoked", functionName);

            if (documents.Count > 0)
            {
                _logger.LogTrace("Attempting to Send {Count} Documents from Address Cosomos DB to Service Bus", documents.Count);
                foreach (var document in documents)
                {
                    try
                    {
                        var changeFeedMessageModel = new ChangeFeedMessageModel()
                        {
                            Document = document,
                            IsAddress = true
                        };
                        var documentId = document.RootElement.GetProperty("id").ToString();
                        _logger.LogTrace("Attempting to send document id: {DocumentID} to service bus queue", documentId);
                        await _serviceBusClient.SendChangeFeedMessageAsync(documentId, changeFeedMessageModel);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error when trying to send message to service bus queue");
                    }
                }
                _logger.LogTrace("Successfully Sent {Count} Documents from Address Cosomos DB to Service Bus", documents.Count);
            }
            else
            {
                _logger.LogInformation("No Documents found from Address Cosomos DB to Process");
            }
            _logger.LogTrace("Function {FunctionName} has finished invoking", functionName);
        }
    }
}