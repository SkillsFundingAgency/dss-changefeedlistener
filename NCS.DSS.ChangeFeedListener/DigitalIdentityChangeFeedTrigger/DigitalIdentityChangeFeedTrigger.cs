using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.ChangeFeedListener.Model;
using NCS.DSS.ChangeFeedListener.ServiceBus;
using System.Text.Json;

namespace NCS.DSS.ChangeFeedListener.DigitalIdentityChangeFeedTrigger
{
    public class DigitalIdentityChangeFeedTrigger
    {
        private readonly IChangeFeedListenerServiceBusClient _serviceBusClient;       
        private readonly ILogger<DigitalIdentityChangeFeedTrigger> _logger;

        private const string DatabaseName = "%DigitalIdentityDatabaseId%";
        private const string CollectionName = "%DigitalIdentityCollectionId%";
        private const string ConnectionString = "CosmosDBConnectionString";
        private const string LeaseCollectionName = "%DigitalIdentityLeaseCollectionName%";
        private const string LeaseCollectionPrefix = "%DigitalIdentityLeaseCollectionPrefix%";

        public DigitalIdentityChangeFeedTrigger(IChangeFeedListenerServiceBusClient serviceBusClient,
            ILogger<DigitalIdentityChangeFeedTrigger> logger)
        {
            _serviceBusClient = serviceBusClient;
            
            _logger = logger;
        }

        [Function("DigitalIdentityChangeFeedTrigger")]
        public async Task Run([CosmosDBTrigger(
            DatabaseName,
            CollectionName,
            Connection = ConnectionString,
            LeaseContainerName = LeaseCollectionName,
            LeaseContainerPrefix = LeaseCollectionPrefix,
            CreateLeaseContainerIfNotExists  = true
            )] IReadOnlyList<JsonDocument> documents)
        {

            var functionName = nameof(DigitalIdentityChangeFeedTrigger);
            _logger.LogInformation("Function {FunctionName} has been invoked", functionName);

            if (documents.Count > 0)
            {
                _logger.LogInformation("Attempting to Send {Count} Documents from Digital Identity Cosomos DB to Service Bus", documents.Count);
                foreach (var document in documents)
                {
                    try
                    {
                        var changeFeedMessageModel = new ChangeFeedMessageModel()
                        {
                            Document = document,
                            IsDigitalIdentity = true
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
                _logger.LogInformation("Successfully Sent {Count} Documents from Digital Identity Cosomos DB to Service Bus", documents.Count);
            }
            else
            {
                _logger.LogInformation("No Documents found from Digital Identity Cosomos DB to Process");
            }
            _logger.LogInformation("Function {FunctionName} has finished invoking", functionName);
        }
    }
}
