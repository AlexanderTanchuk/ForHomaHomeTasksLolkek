using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Linq;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Text;

namespace OrderItemsReserver
{
    public static class OrderItemsReserverFunction
    {
        private static DocumentClient dbClient;
        private static string dbName = "Orders";
        private static string accName = "OrderDetails";


        [FunctionName("OrderItemsReserverFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var order = JsonConvert.DeserializeObject(requestBody);

            await CreateDatabaseAndCollection();
            await CreateOrderIfNotExists(dbName, accName, order);

            return new OkObjectResult(requestBody);
        }

        
        private async static Task CreateDatabaseAndCollection() 
        {
            var accoundEndpoint = Environment.GetEnvironmentVariable("COSMOSBD_ACCOUNT_ENDPOINT");
            var accountKey = Environment.GetEnvironmentVariable("COSMOSBD_ACCOUNT_KEY");
            dbClient = new DocumentClient(new Uri(accoundEndpoint), accountKey);
            await dbClient.CreateDatabaseIfNotExistsAsync(new Database { Id = dbName });
            await dbClient.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri(dbName), new DocumentCollection { Id = accName });
        }

        private async static Task CreateOrderIfNotExists(string databaseName, string collectionName, object order)
        {
            await dbClient.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), order);
        }
    }
}
