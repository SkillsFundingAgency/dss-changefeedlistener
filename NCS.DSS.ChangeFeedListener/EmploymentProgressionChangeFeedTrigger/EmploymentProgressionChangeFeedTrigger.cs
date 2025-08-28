using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.ChangeFeedListener.Constants;
using NCS.DSS.ChangeFeedListener.Model;
using NCS.DSS.ChangeFeedListener.ServiceBus;

namespace NCS.DSS.ChangeFeedListener.EmploymentProgressionChangeFeedTrigger
{
    public class EmploymentProgressionChangeFeedTrigger
    {
        private readonly IChangeFeedListenerServiceBusClient _serviceBusClient;
       
        private readonly ILogger<EmploymentProgressionChangeFeedTrigger> _logger;

        private const string DatabaseName = "%EmploymentProgressionDatabaseId%";
        private const string CollectionName = "%EmploymentProgressionCollectionId%";
        private const string LeaseCollectionName = "%EmploymentProgressionLeaseCollectionName%";
        private const string LeaseCollectionPrefix = "%EmploymentProgressionLeaseCollectionPrefix%";

        public EmploymentProgressionChangeFeedTrigger(IChangeFeedListenerServiceBusClient serviceBusClient,
            
            ILogger<EmploymentProgressionChangeFeedTrigger> logger)
        {
            _serviceBusClient = serviceBusClient;
            
            _logger = logger;
        }

        [Function("EmploymentProgressionChangeFeedTrigger")]
        public async Task Run([CosmosDBTrigger(
            DatabaseName,
            CollectionName,
            Connection = ConfigKeys.CosmosDBConnectionPrefix,
            LeaseContainerName = LeaseCollectionName,
            LeaseContainerPrefix = LeaseCollectionPrefix,
            CreateLeaseContainerIfNotExists  = true
            )] IReadOnlyList<JsonDocument> documents)
        {
            var functionName = nameof(EmploymentProgressionChangeFeedTrigger);
            _logger.LogInformation("Function {FunctionName} has been invoked", functionName);

            if (documents.Count > 0)
            {
                _logger.LogInformation("Attempting to Send {Count} Documents from EmploymentProgression Cosomos DB to Service Bus", documents.Count);
                foreach (var document in documents)
                {
                    try
                    {
                        var changeFeedMessageModel = new ChangeFeedMessageModel()
                        {
                            Document = document,
                            IsEmploymentProgression = true
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
                _logger.LogInformation("Successfully Sent {Count} Documents from EmploymentProgression Cosomos DB to Service Bus", documents.Count);
            }
            else
            {
                _logger.LogInformation("No Documents found from EmploymentProgression Cosomos DB to Process");
            }
            _logger.LogInformation("Function {FunctionName} has finished invoking", functionName);
        }
    }
}