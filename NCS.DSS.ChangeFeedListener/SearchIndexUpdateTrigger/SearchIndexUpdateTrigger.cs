using Azure;
using Azure.Search.Documents.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Customer.Helpers;
using NCS.DSS.Customer.ReferenceData;
using Newtonsoft.Json;
using System.Text.Json;
namespace NCS.DSS.ChangeFeedListener.SearchIndexUpdateTrigger
{
    public class SearchIndexUpdateTrigger
    {
        private readonly ILogger<SearchIndexUpdateTrigger> _logger;

        private const string DatabaseName = "%CustomerDatabaseId%";
        private const string CollectionName = "%CustomerCollectionId%";
        private const string ConnectionString = "CosmosDBConnectionString";
        private const string LeaseCollectionName = "%CustomerLeaseCollectionName%";
        private const string LeaseCollectionPrefix = "Search";

        public SearchIndexUpdateTrigger(ILogger<SearchIndexUpdateTrigger> logger)
        {
            _logger = logger;
        }

        [Function("SearchIndexUpdateTrigger")]
        public async Task Run([CosmosDBTrigger(
            DatabaseName,
            CollectionName,
            Connection = ConnectionString,
            LeaseContainerName = LeaseCollectionName,
            LeaseContainerPrefix = LeaseCollectionPrefix,
            CreateLeaseContainerIfNotExists  = true
            )] IReadOnlyList<JsonDocument> documents)
        {
            _logger.LogInformation("SearchIndexUpdateTrigger fired.");


            _logger.LogInformation("Getting search service client");

            var indexClient = SearchHelper.GetSearchServiceClient(); ;
            var indexClientV2 = SearchHelper.GetSearchServiceClientV2();

            _logger.LogInformation("Retrieved index client");

            if (documents != null && documents.Count > 0)
            {
                
                var customers = documents.Select(doc => JsonConvert.DeserializeObject<Model.Customer>(doc.RootElement.GetRawText()))                 
                .ToList();

                var customersV2 = documents.Select(doc => JsonConvert.DeserializeObject<Model.CustomerSearch>(doc.RootElement.GetRawText()))
                .ToList();

                try
                {
                    _logger.LogInformation("attempting to merge docs to azure search");
                    var batch = IndexDocumentsBatch.MergeOrUpload(customers);
                    await indexClient.IndexDocumentsAsync(batch);

                    _logger.LogInformation("successfully merged docs to azure search");

                }
                catch (RequestFailedException e)
                {

                    _logger.LogError(e, "Failed to update search");
                }
                try
                {
                    _logger.LogInformation("attempting to merge docs to azure search V2");
                    //V2
                    var batch = IndexDocumentsBatch.MergeOrUpload(customersV2);
                    await indexClientV2.IndexDocumentsAsync(batch);

                    _logger.LogInformation("successfully merged docs to azure search V2");

                }
                catch (RequestFailedException e)
                {
                    _logger.LogError(e,"Failed to update search");
                }
            }
        }
    }
}
