using Azure;
using Azure.Search.Documents.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.ChangeFeedListener.Constants;
using NCS.DSS.Customer.Helpers;
using Newtonsoft.Json;
using System.Text.Json;
namespace NCS.DSS.ChangeFeedListener.SearchIndexUpdateTrigger
{
    public class SearchIndexUpdateTrigger
    {
        private readonly ILogger<SearchIndexUpdateTrigger> _logger;

        private const string DatabaseName = "%CustomerDatabaseId%";
        private const string CollectionName = "%CustomerCollectionId%";
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
            Connection = ConfigKeys.CosmosDBConnectionPrefix,
            LeaseContainerName = LeaseCollectionName,
            LeaseContainerPrefix = LeaseCollectionPrefix,
            CreateLeaseContainerIfNotExists  = true
            )] IReadOnlyList<JsonDocument> documents)
        {
            var functionName = nameof(SearchIndexUpdateTrigger);
            _logger.LogInformation("Function {FunctionName} has been invoked", functionName);

            _logger.LogInformation("Attempting get search service client");

            var indexClient = SearchHelper.GetSearchServiceClient(); ;
            var indexClientV2 = SearchHelper.GetSearchServiceClientV2();

            _logger.LogInformation("Retrieved Search Service client");

            if (documents != null && documents.Count > 0)
            {                
                var customers = documents.Select(doc => JsonConvert.DeserializeObject<Model.Customer>(doc.RootElement.GetRawText()))                 
                .ToList();

                var customerSearch = documents.Select(doc => JsonConvert.DeserializeObject<Model.CustomerSearch>(doc.RootElement.GetRawText()))
                .ToList();

                try
                {
                    _logger.LogInformation("Attempting to merge {Count} Customer docs to azure search",customers.Count);

                    var batch = IndexDocumentsBatch.MergeOrUpload(customers);
                    await indexClient.IndexDocumentsAsync(batch);

                    _logger.LogInformation("Successfully merged {Count} Customer docs to azure search",customers.Count);

                }
                catch (RequestFailedException e)
                {
                    _logger.LogError(e, "Failed to update Customer Docs");
                    throw;
                }
                try
                {
                    _logger.LogInformation("Attempting to merge {Count} Customer Search docs to azure search",customerSearch.Count);

                    var batch = IndexDocumentsBatch.MergeOrUpload(customerSearch);
                    await indexClientV2.IndexDocumentsAsync(batch);

                    _logger.LogInformation("Successfully merged {Count} Customer Search docs to azure search", customerSearch.Count);

                }
                catch (RequestFailedException e)
                {
                    _logger.LogError(e, "Failed to update Customer Search Docs");
                    throw;
                }
            }
            else
            {
                _logger.LogWarning("No Documents found to Update Search Index");
            }
                
            _logger.LogInformation("Function {FunctionName} has finished invoking", functionName);
        }
    }
}
