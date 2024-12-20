using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.ChangeFeedListener.Model;
using NCS.DSS.ChangeFeedListener.ServiceBus;

namespace NCS.DSS.ChangeFeedListener.SubscriptionChangeFeedTrigger
{
    public class SubscriptionChangeFeedTrigger
    {
        private readonly IChangeFeedListenerServiceBusClient _serviceBusClient;
       
        private readonly ILogger _logger;

        private const string DatabaseName = "%SubscriptionDatabaseId%";
        private const string CollectionName = "%SubscriptionCollectionId%";
        private const string ConnectionString = "CosmosDBConnectionString";
        private const string LeaseCollectionName = "%SubscriptionLeaseCollectionName%";
        private const string LeaseCollectionPrefix = "%SubscriptionLeaseCollectionPrefix%";

        public SubscriptionChangeFeedTrigger(IChangeFeedListenerServiceBusClient serviceBusClient,
            
            ILogger<SubscriptionChangeFeedTrigger> logger)
        {
            _serviceBusClient = serviceBusClient;
            
            _logger = logger;
        }

        [Function("SubscriptionChangeFeedTrigger")]
        public async Task Run([CosmosDBTrigger(
            DatabaseName,
            CollectionName,
            Connection = ConnectionString,
            LeaseContainerName = LeaseCollectionName,
            LeaseContainerPrefix = LeaseCollectionPrefix,
            CreateLeaseContainerIfNotExists  = true
            )] IReadOnlyList<JsonDocument> documents)
        {
            var functionName = nameof(SubscriptionChangeFeedTrigger);
            _logger.LogInformation("Function {FunctionName} has been invoked", functionName);

            if (documents.Count > 0)
            {
                _logger.LogInformation("Attempting to Send {Count} Documents from Subscription Cosomos DB to Service Bus", documents.Count);
                foreach (var document in documents)
                {
                    try
                    {
                        var changeFeedMessageModel = new ChangeFeedMessageModel()
                        {
                            Document = document,
                            IsSubscription = true
                        };
                        var documentId = document.RootElement.GetProperty("id").ToString();
                        _logger.LogInformation("Attempting to send document id: {DocumentID} to service bus queue", documentId);
                        await _serviceBusClient.SendChangeFeedMessageAsync(documentId, changeFeedMessageModel);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error when trying to send message to service bus queue");
                    }
                }
                _logger.LogInformation("Successfully Sent {Count} Documents from Subscription Cosomos DB to Service Bus", documents.Count);
            }
            else
            {
                _logger.LogInformation("No Documents found from Subscription Cosomos DB to Process");
            }
            _logger.LogInformation("Function {FunctionName} has finished invoking", functionName);
        }

    }
}