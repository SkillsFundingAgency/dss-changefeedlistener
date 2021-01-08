using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.Common.Standard.Logging;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NCS.DSS.ChangeFeedListener.Model;
using NCS.DSS.ChangeFeedListener.ServiceBus;

namespace NCS.DSS.ChangeFeedListener.ActionPlanChangeFeedTrigger
{
    public class ActionPlanChangeFeedTrigger
    {
        private readonly IServiceBusClient _serviceBusClient;
        private readonly ILoggerHelper _loggerHelper;

        private const string DatabaseName = "%ActionPlanDatabaseId%";
        private const string CollectionName = "%ActionPlanCollectionId%";
        private const string ConnectionString = "CosmosDBConnectionString";
        private const string LeaseCollectionName = "%ActionPlanLeaseCollectionName%";
        private const string LeaseCollectionPrefix = "%ActionPlanLeaseCollectionPrefix%";

        public ActionPlanChangeFeedTrigger(IServiceBusClient serviceBusClient, ILoggerHelper loggerHelper)
        {
            _serviceBusClient = serviceBusClient;
            _loggerHelper = loggerHelper;
        }

        [FunctionName("ActionPlanChangeFeedTrigger")]
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
                        IsActionPlan = true
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