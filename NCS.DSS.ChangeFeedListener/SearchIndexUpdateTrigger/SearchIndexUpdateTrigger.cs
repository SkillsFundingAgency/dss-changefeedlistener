using Azure;
using Azure.Search.Documents.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Customer.Helpers;
using NCS.DSS.Customer.ReferenceData;
using Document = Microsoft.Azure.Documents.Document;

namespace NCS.DSS.ChangeFeedListener.SearchIndexUpdateTrigger
{
    public class SearchIndexUpdateTrigger
    {
        private readonly ILogger _logger;

        private const string DatabaseName = "%CustomerDatabaseId%";
        private const string CollectionName = "%CustomerCollectionId%";
        private const string ConnectionString = "CosmosDBConnectionString";
        private const string LeaseCollectionName = "%CustomerLeaseCollectionName%";
        private const string LeaseCollectionPrefix = "Search";

        public SearchIndexUpdateTrigger(ILogger logger)
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
            )] IReadOnlyList<Document> documents)
        {
            _logger.LogInformation("SearchIndexUpdateTrigger fired.");


            _logger.LogInformation("Getting search service client");

            var indexClient = SearchHelper.GetSearchServiceClient(); ;
            var indexClientV2 = SearchHelper.GetSearchServiceClientV2();

            _logger.LogInformation("Retrieved index client");

            if (documents != null && documents.Count > 0)
            {
                var customers = documents.Select(doc => new Model.Customer()
                {
                    Id = doc.GetPropertyValue<Guid?>("id"),
                    DateOfRegistration = doc.GetPropertyValue<DateTime?>("DateOfRegistration"),
                    GivenName = doc.GetPropertyValue<string>("GivenName"),
                    FamilyName = doc.GetPropertyValue<string>("FamilyName"),
                    DateofBirth = doc.GetPropertyValue<DateTime?>("DateofBirth"),
                    UniqueLearnerNumber = doc.GetPropertyValue<string>("UniqueLearnerNumber"),
                    OptInUserResearch = doc.GetPropertyValue<bool?>("OptInUserResearch"),
                    OptInMarketResearch = doc.GetPropertyValue<bool?>("OptInMarketResearch"),
                    DateOfTermination = doc.GetPropertyValue<DateTime?>("DateOfTermination"),
                    ReasonForTermination = doc.GetPropertyValue<ReasonForTermination?>("ReasonForTermination"),
                    IntroducedBy = doc.GetPropertyValue<IntroducedBy?>("IntroducedBy"),
                    IntroducedByAdditionalInfo = doc.GetPropertyValue<string>("IntroducedByAdditionalInfo"),
                    LastModifiedDate = doc.GetPropertyValue<DateTime?>("LastModifiedDate"),
                    LastModifiedTouchpointId = doc.GetPropertyValue<string>("LastModifiedTouchpointId")
                }).ToList();

                var customersV2 = documents.Select(doc => new Model.CustomerSearch()
                {
                    CustomerId = doc.GetPropertyValue<Guid?>("id"),
                    DateOfRegistration = doc.GetPropertyValue<DateTime?>("DateOfRegistration"),
                    Title = doc.GetPropertyValue<Title>("Title"),
                    GivenName = doc.GetPropertyValue<string>("GivenName"),
                    FamilyName = doc.GetPropertyValue<string>("FamilyName"),
                    DateofBirth = doc.GetPropertyValue<DateTime?>("DateofBirth"),
                    Gender = doc.GetPropertyValue<Gender?>("Gender"),
                    UniqueLearnerNumber = doc.GetPropertyValue<string>("UniqueLearnerNumber"),
                    OptInUserResearch = doc.GetPropertyValue<bool?>("OptInUserResearch"),
                    OptInMarketResearch = doc.GetPropertyValue<bool?>("OptInMarketResearch"),
                    DateOfTermination = doc.GetPropertyValue<DateTime?>("DateOfTermination"),
                    ReasonForTermination = doc.GetPropertyValue<ReasonForTermination?>("ReasonForTermination"),
                    IntroducedBy = doc.GetPropertyValue<IntroducedBy?>("IntroducedBy"),
                    IntroducedByAdditionalInfo = doc.GetPropertyValue<string>("IntroducedByAdditionalInfo"),
                    LastModifiedDate = doc.GetPropertyValue<DateTime?>("LastModifiedDate"),
                    LastModifiedTouchpointId = doc.GetPropertyValue<string>("LastModifiedTouchpointId")
                }).ToList();

                try
                {
                    _logger.LogInformation("attempting to merge docs to azure search");
                    var batch = IndexDocumentsBatch.MergeOrUpload(customers);
                    await indexClient.IndexDocumentsAsync(batch);

                    _logger.LogInformation("successfully merged docs to azure search");

                }
                catch (RequestFailedException e)
                {

                    _logger.LogError("Failed to update search", e);
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
                    _logger.LogError("Failed to update search", e);
                }
            }
        }
    }
}
