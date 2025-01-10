using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Azure.Identity;

public static class GetDocumentById
{
    private static string endpoint = Environment.GetEnvironmentVariable("COSMOS_ENDPOINT", EnvironmentVariableTarget.Process);
    private static CosmosClient cosmosClient = new CosmosClient(endpoint, new DefaultAzureCredential());
    private static Database database = cosmosClient.GetDatabase("ToDoList");
    private static Container container = database.GetContainer("Items");
    private static string partitionKey = "household";

    [Function("GetDocumentById")]
    public static async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "documents/{id}")] HttpRequestData req,
        string id,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("GetDocumentById");
        logger.LogInformation($"Retrieving document with ID: {id}");


        var response = req.CreateResponse(HttpStatusCode.OK);

        try
        {
            ItemResponse<dynamic> documentResponse = await container.ReadItemAsync<dynamic>(id, new PartitionKey(partitionKey));
            JObject document = documentResponse.Resource as JObject;

            if (document != null)
            {
                response.Headers.Add("Content-Type", "application/json");
                await response.WriteStringAsync(document.ToString());
            }
            else
            {
                response = req.CreateResponse(HttpStatusCode.NotFound);
                await response.WriteStringAsync($"Document with ID {id} not found.");
            }
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            response = req.CreateResponse(HttpStatusCode.NotFound);
            await response.WriteStringAsync($"Document with ID {id} not found.");
        }

        return response;
    }
}