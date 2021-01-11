using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.Common.Standard.Logging;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NCS.DSS.ChangeFeedListener.Model;
using NCS.DSS.ChangeFeedListener.ServiceBus;

namespace NCS.DSS.ChangeFeedListener.ActionChangeFeedTrigger
{
    public class ActionChangeFeedTrigger
    {
        private readonly IServiceBusClient _serviceBusClient;
        private readonly ILoggerHelper _loggerHelper;

        private const string DatabaseName = "%ActionDatabaseId%";
        private const string CollectionName = "%ActionCollectionId%";
        private const string ConnectionString = "CosmosDBConnectionString";
        private const string LeaseCollectionName = "%ActionLeaseCollectionName%";
        private const string LeaseCollectionPrefix = "%ActionLeaseCollectionPrefix%";

        public ActionChangeFeedTrigger(IServiceBusClient serviceBusClient, ILoggerHelper loggerHelper)
        {
            _serviceBusClient = serviceBusClient;
            _loggerHelper = loggerHelper;
        }

        [FunctionName("ActionChangeFeedTrigger")]
        public async Task Run([CosmosDBTrigger(
            DatabaseName,
            CollectionName,
            ConnectionStringSetting = ConnectionString,
            LeaseCollectionName = LeaseCollectionName,
            LeaseCollectionPrefix = LeaseCollectionPrefix,
            CreateLeaseCollectionIfNotExists = true
            )] IReadOnlyList<Document> documents,
            ILogger log)
        {
            try
            {
                foreach (var document in documents)
                {
                    var changeFeedMessageModel = new ChangeFeedMessageModel()
                    {
                        Document = document,
                        IsAction = true
                    };

                    _loggerHelper.LogInformationMessage(log, Guid.NewGuid(), string.Format("Attempting to send document id: {0} to service bus queue", document.Id));
                    await _serviceBusClient.SendChangeFeedMessageAsync(document, changeFeedMessageModel);
                }
            }
            catch (Exception ex)
            {
                _loggerHelper.LogException(log, Guid.NewGuid(), "Error when trying to send message to service bus queue", ex);
            }
        }
    }
}