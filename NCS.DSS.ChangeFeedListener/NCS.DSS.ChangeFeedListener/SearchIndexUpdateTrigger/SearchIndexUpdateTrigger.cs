using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DFC.Common.Standard.Logging;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace NCS.DSS.ChangeFeedListener.SearchIndexUpdateTrigger
{
    public class SearchIndexUpdateTrigger
    {
        private readonly ILoggerHelper _loggerHelper;

        private const string DatabaseName = "%CustomerDatabaseId%";
        private const string CollectionName = "%CustomerCollectionId%";
        private const string ConnectionString = "CosmosDBConnectionString";

        public SearchIndexUpdateTrigger(ILoggerHelper loggerHelper)
        {
            _loggerHelper = loggerHelper;
        }

        [FunctionName("SearchIndexUpdateTrigger")]
        public async Task Run([CosmosDBTrigger(
            DatabaseName,
            CollectionName,
            ConnectionStringSetting = ConnectionString,
            CreateLeaseCollectionIfNotExists = true
            )] IReadOnlyList<Document> documents,
            ILogger log)
        {
            if (documents != null && documents.Count > 0)
            {
                log.LogInformation("Documents modified " + documents.Count);
                try
                {
                    foreach (var document in documents)
                    {


                        log.LogInformation("Document Id " + document.Id);

                    }
                    log.LogInformation("Getting search index...");
                }
                catch (Exception ex)
                {
                    _loggerHelper.LogException(log, Guid.NewGuid(), "Error when trying to fetch chenged documents", ex);
                }
            }
        }
    }
}
