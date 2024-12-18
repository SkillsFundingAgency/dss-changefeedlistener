using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.ChangeFeedListener.Model;
using NCS.DSS.ChangeFeedListener.ServiceBus;
using System.Text.Json;

namespace NCS.DSS.ChangeFeedListener.ActionPlanChangeFeedTrigger
{
    public class ActionPlanChangeFeedTrigger
    {
        private readonly IChangeFeedListenerServiceBusClient _serviceBusClient;
        private readonly ILogger<ActionPlanChangeFeedTrigger> _logger;

        private const string DatabaseName = "%ActionPlanDatabaseId%";
        private const string CollectionName = "%ActionPlanCollectionId%";
        private const string ConnectionString = "CosmosDBConnectionString";
        private const string LeaseCollectionName = "%ActionPlanLeaseCollectionName%";
        private const string LeaseCollectionPrefix = "%ActionPlanLeaseCollectionPrefix%";

        public ActionPlanChangeFeedTrigger(IChangeFeedListenerServiceBusClient serviceBusClient,
            ILogger<ActionPlanChangeFeedTrigger> logger)
        {
            _serviceBusClient = serviceBusClient;
            _logger = logger;
        }

        [Function("ActionPlanChangeFeedTrigger")]
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
                        IsActionPlan = true
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