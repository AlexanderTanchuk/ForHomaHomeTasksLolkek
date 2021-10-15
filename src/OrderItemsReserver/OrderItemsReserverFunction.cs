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
            /*[StorageAccount("AzureWebJobsStorage")] CloudStorageAccount account,*/
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            
            
            
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var order = JsonConvert.DeserializeObject(requestBody);

            //#######Azure Function and Blob Storage Hometask###########
            /*var containerName = "order"+Guid.NewGuid().ToString("n");
            await CreateBlob(containerName, requestBody);*/
            //##########################################################

            await CreateDatabaseAndCollection();
            await CreateOrderIfNotExists(dbName, accName, order);


            return new OkObjectResult(requestBody);
        }

        private async static Task CreateBlob(string name, string data)
        {
            string connectionString;
            CloudStorageAccount storageAccount;
            CloudBlobClient client;
            CloudBlobContainer container;
            CloudBlockBlob blob;

            connectionString = "DefaultEndpointsProtocol=https;AccountName=orderitemblobstorage;AccountKey=XaEJ+33xZG/lW7ecucZOzNxq6vbabPIUrhNlH/3UOj2svkgMQlJ59lh/Dgal2ICV65ASPpAesWl54biREWtRTg==;EndpointSuffix=core.windows.net";
            storageAccount = CloudStorageAccount.Parse(connectionString);

            client = storageAccount.CreateCloudBlobClient();

            container = client.GetContainerReference("order");

            await container.CreateIfNotExistsAsync();

            blob = container.GetBlockBlobReference(name);
            blob.Properties.ContentType = "application/json";

            using (Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(data)))
            {
                await blob.UploadFromStreamAsync(stream);
            }
        }

        private async static Task CreateDatabaseAndCollection() 
        {
            var accoundEndpoint = "https://azorderscosmos.documents.azure.com:443/";
            var accountKey = "jNTqCBCyvCyVh9sqF1xsFjMf4vxAK9vy57XFE8ZzJcSSrMtyoENf5DVp3wIVXrv1oRPczXNdzgWqewSY7EaULA==";
            dbClient = new DocumentClient(new Uri(accoundEndpoint), accountKey);
            await dbClient.CreateDatabaseIfNotExistsAsync(new Database { Id = dbName });
            await dbClient.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri("Orders"), new DocumentCollection { Id = accName });
        }

        private async static Task CreateOrderIfNotExists(string databaseName, string collectionName, object test)
        {
            await dbClient.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), test);
        }
    }
}
