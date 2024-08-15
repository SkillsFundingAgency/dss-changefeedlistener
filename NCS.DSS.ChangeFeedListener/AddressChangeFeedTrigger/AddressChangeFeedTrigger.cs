using DFC.Common.Standard.Logging;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.ChangeFeedListener.Model;
using NCS.DSS.ChangeFeedListener.ServiceBus;
using Newtonsoft.Json;

namespace NCS.DSS.ChangeFeedListener.AddressChangeFeedTrigger
{
    public class AddressChangeFeedTrigger
    {
        private readonly IServiceBusClient _serviceBusClient;
        private readonly ILoggerHelper _loggerHelper;
        private readonly ILogger _logger;

        private const string DatabaseName = "%AddressDatabaseId%";
        private const string CollectionName = "%AddressCollectionId%";
        private const string ConnectionString = "CosmosDBConnectionString";
        private const string LeaseCollectionName = "%AddressLeaseCollectionName%";
        private const string LeaseCollectionPrefix = "%AddressLeaseCollectionPrefix%";

        public AddressChangeFeedTrigger(IServiceBusClient serviceBusClient, 
            ILoggerHelper loggerHelper, 
            ILogger<AddressChangeFeedTrigger> logger)
        {
            _serviceBusClient = serviceBusClient;
            _loggerHelper = loggerHelper;
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
            )] IReadOnlyList<Document> documents)
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
                    
                    var coorelationId = Guid.NewGuid();

                    var messageModel = JsonConvert.SerializeObject(changeFeedMessageModel);
                    _loggerHelper.LogInformationMessage(_logger, coorelationId, $"Message Mode: {messageModel}");

                    var documentModel = JsonConvert.SerializeObject(document);
                    _loggerHelper.LogInformationMessage(_logger, coorelationId, $"Message Mode: {documentModel}");

                    _loggerHelper.LogInformationMessage(_logger, Guid.NewGuid(), string.Format("Attempting to send document id: {0} to service bus queue", document.Id));
                    await _serviceBusClient.SendChangeFeedMessageAsync(document, changeFeedMessageModel);
                }
            }
            catch (Exception ex)
            {
                _loggerHelper.LogException(_logger, Guid.NewGuid(), "Error when trying to send message to service bus queue", ex);
            }
        }
    }
}