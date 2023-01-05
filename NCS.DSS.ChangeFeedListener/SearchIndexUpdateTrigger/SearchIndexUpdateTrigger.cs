using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using DFC.Common.Standard.Logging;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Azure.Search;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Logging;
using NCS.DSS.Customer.Helpers;
using Document = Microsoft.Azure.Documents.Document;
using NCS.DSS.Customer.ReferenceData;

namespace NCS.DSS.ChangeFeedListener.SearchIndexUpdateTrigger
{
    public class SearchIndexUpdateTrigger
    {
        private readonly ILoggerHelper _loggerHelper;

        private const string DatabaseName = "%CustomerDatabaseId%";
        private const string CollectionName = "%CustomerCollectionId%";
        private const string ConnectionString = "CosmosDBConnectionString";
        private const string LeaseCollectionName = "%CustomerLeaseCollectionName%";
        private const string LeaseCollectionPrefix = "Search";

        public SearchIndexUpdateTrigger(ILoggerHelper loggerHelper)
        {
            _loggerHelper = loggerHelper;
        }

        [FunctionName("SearchIndexUpdateTrigger")]
        public async Task Run([CosmosDBTrigger(
            DatabaseName,
            CollectionName,
            Connection = ConnectionString,
            LeaseContainerName = LeaseCollectionName,
            LeaseContainerPrefix = LeaseCollectionPrefix,
            CreateLeaseContainerIfNotExists  = true
            )] IReadOnlyList<Document> documents,
            ILogger log)
        {
            log.LogInformation("SearchIndexUpdateTrigger fired.");

            
            log.LogInformation("Getting search service client");

            var indexClient = SearchHelper.GetSearchServiceClient(); ;
            var indexClientV2 = SearchHelper.GetSearchServiceClientV2();

            log.LogInformation("Retrieved index client");

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

                var batch = IndexDocumentsBatch.MergeOrUpload(customers);
                var batchV2 = IndexDocumentsBatch.MergeOrUpload(customersV2);

                try
                {
                    log.LogInformation("attempting to merge docs to azure search");

                    await indexClient.IndexDocumentsAsync(batch);

                    log.LogInformation("successfully merged docs to azure search");

                }
                catch (RequestFailedException e)
                {

                    log.LogError("Failed to update search", e);
                }
                try
                {
                    log.LogInformation("attempting to merge docs to azure search V2");
                    //V2
                    await indexClientV2.IndexDocumentsAsync(batchV2);

                    log.LogInformation("successfully merged docs to azure search V2");

                }
                catch (RequestFailedException e)
                {
                    log.LogError("Failed to update search", e);
                }
            }
        }
    }
}
