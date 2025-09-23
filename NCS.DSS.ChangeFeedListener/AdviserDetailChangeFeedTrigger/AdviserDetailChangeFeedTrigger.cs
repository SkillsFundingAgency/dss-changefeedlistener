using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.ChangeFeedListener.Constants;
using NCS.DSS.ChangeFeedListener.Model;
using NCS.DSS.ChangeFeedListener.ServiceBus;
using System.Text.Json;

namespace NCS.DSS.ChangeFeedListener.AdviserDetailChangeFeedTrigger
{
    public class AdviserDetailChangeFeedTrigger
    {
        private readonly IChangeFeedListenerServiceBusClient _serviceBusClient;
        private readonly ILogger<AdviserDetailChangeFeedTrigger> _logger;

        private const string DatabaseName = "%AdviserDetailDatabaseId%";
        private const string CollectionName = "%AdviserDetailCollectionId%";
        private const string LeaseCollectionName = "%AdviserDetailLeaseCollectionName%";
        private const string LeaseCollectionPrefix = "%AdviserDetailLeaseCollectionPrefix%";

        public AdviserDetailChangeFeedTrigger(IChangeFeedListenerServiceBusClient serviceBusClient,
            ILogger<AdviserDetailChangeFeedTrigger> logger)
        {
            _serviceBusClient = serviceBusClient;
            _logger = logger;
        }

        [Function("AdviserDetailChangeFeedTrigger")]
        public async Task Run([CosmosDBTrigger(
            DatabaseName,
            CollectionName,
            Connection = ConfigKeys.CosmosDBConnectionPrefix,
            LeaseContainerName = LeaseCollectionName,
            LeaseContainerPrefix = LeaseCollectionPrefix,
            CreateLeaseContainerIfNotExists  = true
            )] IReadOnlyList<JsonDocument> documents)
        {

            var functionName = nameof(AdviserDetailChangeFeedTrigger);
            _logger.LogTrace("Function {FunctionName} has been invoked", functionName);

            if (documents.Count > 0)
            {
                _logger.LogTrace("Attempting to Send {Count} Documents from Adviser Detail Cosomos DB to Service Bus", documents.Count);
                foreach (var document in documents)
                {
                    try
                    {
                        var changeFeedMessageModel = new ChangeFeedMessageModel()
                        {
                            Document = document,
                            IsAdviserDetail = true
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
                _logger.LogTrace("Successfully Sent {Count} Documents from Adviser Detail Cosomos DB to Service Bus", documents.Count);
            }
            else
            {
                _logger.LogInformation("No Documents found from Adviser Detail Cosomos DB to Process");
            }
            _logger.LogTrace("Function {FunctionName} has finished invoking", functionName);
        }
    }
}