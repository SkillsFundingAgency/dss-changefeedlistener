using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.ChangeFeedListener.Constants;
using NCS.DSS.ChangeFeedListener.Model;
using NCS.DSS.ChangeFeedListener.ServiceBus;

namespace NCS.DSS.ChangeFeedListener.TransferChangeFeedTrigger
{
    public class TransferChangeFeedTrigger
    {
        private readonly IChangeFeedListenerServiceBusClient _serviceBusClient;
       
        private readonly ILogger<TransferChangeFeedTrigger> _logger;

        private const string DatabaseName = "%TransferDatabaseId%";
        private const string CollectionName = "%TransferCollectionId%";
        private const string LeaseCollectionName = "%TransferLeaseCollectionName%";
        private const string LeaseCollectionPrefix = "%TransferLeaseCollectionPrefix%";

        public TransferChangeFeedTrigger(IChangeFeedListenerServiceBusClient serviceBusClient,            
            ILogger<TransferChangeFeedTrigger> logger)
        {
            _serviceBusClient = serviceBusClient;
            
            _logger = logger;
        }

        [Function("TransferChangeFeedTrigger")]
        public async Task Run([CosmosDBTrigger(
            DatabaseName,
            CollectionName,
            Connection = ConfigKeys.CosmosDBConnectionPrefix,
            LeaseContainerName = LeaseCollectionName,
            LeaseContainerPrefix = LeaseCollectionPrefix,
            CreateLeaseContainerIfNotExists  = true
            )] IReadOnlyList<JsonDocument> documents)
        {
            var functionName = nameof(TransferChangeFeedTrigger);
            _logger.LogTrace("Function {FunctionName} has been invoked", functionName);

            if (documents.Count > 0)
            {
                _logger.LogTrace("Attempting to Send {Count} Documents from Transfer Cosomos DB to Service Bus", documents.Count);
                foreach (var document in documents)
                {
                    try
                    {
                        var changeFeedMessageModel = new ChangeFeedMessageModel()
                        {
                            Document = document,
                            IsTransfer = true
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
                _logger.LogTrace("Successfully Sent {Count} Documents from Transfer Cosomos DB to Service Bus", documents.Count);
            }
            else
            {
                _logger.LogInformation("No Documents found from Transfer Cosomos DB to Process");
            }
            _logger.LogTrace("Function {FunctionName} has finished invoking", functionName);
        }
    }
}