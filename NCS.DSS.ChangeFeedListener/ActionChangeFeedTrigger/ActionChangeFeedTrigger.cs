using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.ChangeFeedListener.Constants;
using NCS.DSS.ChangeFeedListener.Model;
using NCS.DSS.ChangeFeedListener.ServiceBus;
using System.Text.Json;

namespace NCS.DSS.ChangeFeedListener.ActionChangeFeedTrigger
{
    public class ActionChangeFeedTrigger
    {
        private readonly IChangeFeedListenerServiceBusClient _serviceBusClient;
        private readonly ILogger<ActionChangeFeedTrigger> _logger;
        private const string DatabaseName = "%ActionDatabaseId%";
        private const string CollectionName = "%ActionCollectionId%";
        private const string LeaseCollectionName = "%ActionLeaseCollectionName%";
        private const string LeaseCollectionPrefix = "%ActionLeaseCollectionPrefix%";

        public ActionChangeFeedTrigger(IChangeFeedListenerServiceBusClient serviceBusClient,
            ILogger<ActionChangeFeedTrigger> logger)
        {
            _serviceBusClient = serviceBusClient;
            _logger = logger;
        }

        [Function("ActionChangeFeedTrigger")]
        public async Task Run([CosmosDBTrigger(
                DatabaseName,
                CollectionName,
                Connection = ConfigKeys.CosmosDBConnectionPrefix,
                LeaseContainerName = LeaseCollectionName,
                LeaseContainerPrefix = LeaseCollectionPrefix,
                CreateLeaseContainerIfNotExists  = true
            )] IReadOnlyList<JsonDocument> documents
            )
        {
            var functionName = nameof(ActionChangeFeedTrigger);
            _logger.LogTrace("Function {FunctionName} has been invoked", functionName);

            if (documents.Count > 0) {
                _logger.LogTrace("Attempting to Send {Count} Documents from Action Cosomos DB to Service Bus", documents.Count);
                foreach (var document in documents)
                {
                    try
                    {
                        var changeFeedMessageModel = new ChangeFeedMessageModel()
                        {
                            Document = document,
                            IsAction = true
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
                _logger.LogTrace("Successfully Sent {Count} Documents from Action Cosomos DB to Service Bus", documents.Count);
            }
            else
            {
                _logger.LogInformation("No Documents found from Action Cosomos DB to Process");
            }
            _logger.LogTrace("Function {FunctionName} has finished invoking", functionName);
        }
    }
}