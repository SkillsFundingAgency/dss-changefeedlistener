﻿using Azure;
using Azure.Search.Documents;

namespace NCS.DSS.Customer.Helpers
{
    public static class SearchHelper
    {
        private static readonly string SearchServiceName = Environment.GetEnvironmentVariable("SearchServiceName");
        private static readonly string SearchServiceKey = Environment.GetEnvironmentVariable("SearchServiceAdminApiKey");
        private static readonly string SearchServiceIndexName = Environment.GetEnvironmentVariable("CustomerSearchIndexName");
        private static readonly string SearchServiceIndexNameV2 = Environment.GetEnvironmentVariable("CustomerSearchIndexNameV2");

        private static SearchClient _client;
        private static SearchClient _client2;


        public static SearchClient GetSearchServiceClient()
        {
            if (_client != null)
                return _client;

            _client = new SearchClient(new Uri($"https://{SearchServiceName}.search.windows.net"), SearchServiceIndexName, new AzureKeyCredential(SearchServiceKey));

            return _client;
        }

        public static SearchClient GetSearchServiceClientV2()
        {
            if (_client2 != null)
                return _client2;

            _client2 = new SearchClient(new Uri($"https://{SearchServiceName}.search.windows.net"), SearchServiceIndexNameV2, new AzureKeyCredential(SearchServiceKey));

            return _client2;
        }

    }
}