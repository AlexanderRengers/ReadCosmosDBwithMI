using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Azure.Identity;

namespace ReadCosmosDBwithMI
{
    public class readdatabases
    {
        private readonly ILogger<readdatabases> _logger;

        public readdatabases(ILogger<readdatabases> logger)
        {
            _logger = logger;
        }

        [Function("readdatabases")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogTrace("Start function");

            string endpoint = Environment.GetEnvironmentVariable("COSMOS_ENDPOINT", EnvironmentVariableTarget.Process);
            if (string.IsNullOrEmpty(endpoint))
            {
                _logger.LogError("COSMOS_ENDPOINT environment variable is not set.");
                return new BadRequestObjectResult("COSMOS_ENDPOINT environment variable is not set.");
            }

            CosmosClient client = new CosmosClient(endpoint, new DefaultAzureCredential());

            using FeedIterator<DatabaseProperties> iterator = client.GetDatabaseQueryIterator<DatabaseProperties>();

            List<(string name, string uri)> databases = new();
            while (iterator.HasMoreResults)
            {
                FeedResponse<DatabaseProperties> response = await iterator.ReadNextAsync();
                _logger.LogTrace($"Number of databases retrieved: {response.Count}");

                foreach (DatabaseProperties database in response)
                {
                    _logger.LogTrace($"[Database Found]\t{database.Id}");
                    databases.Add((database.Id, database.SelfLink));
                }
            }

            if (databases.Count == 0)
            {
                _logger.LogWarning("No databases found.");
            }

            return new OkObjectResult(databases);
        }
    }
}