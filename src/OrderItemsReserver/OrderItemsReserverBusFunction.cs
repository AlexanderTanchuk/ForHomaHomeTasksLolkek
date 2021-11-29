using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace OrderItemsReserver
{
    public static class OrderItemsReserverBusFunction
    {
        [FunctionName("OrderItemsReserverBusFunction")]
        public static async Task Run([ServiceBusTrigger("pendingorders")]string myQueueItem, ILogger log)
        {
            var containerName = "order"+Guid.NewGuid().ToString("n");
            var attempt = 0;
            while (attempt < 3) 
            {
                try
                {
                    await CreateBlob(containerName, myQueueItem);
                    return;
                }
                catch
                {
                    attempt++;
                    log.LogError($"Failed to create blob. {attempt} attempts have been made");
                }
            }

            log.LogInformation($"Sending order to ServiceBus");
            await SendToServiceBusAsync(myQueueItem);
        }

        private async static Task CreateBlob(string name, string data)
        {
            string connectionString;
            CloudStorageAccount storageAccount;
            CloudBlobClient client;
            CloudBlobContainer container;
            CloudBlockBlob blob;
            var storageVariableName = "AzureWebJobsStorage";

            connectionString = Environment.GetEnvironmentVariable(storageVariableName);
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

        private async static Task SendToServiceBusAsync(string order)
        {
            var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsServiceBus"); ;
            var queueName = "failedorders";
            IQueueClient queueClient;

            string messageBody = order;
            var message = new Message(Encoding.UTF8.GetBytes(messageBody));
            queueClient = new QueueClient(connectionString, queueName);
            try
            {
                await queueClient.SendAsync(message);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
            }

            await queueClient.CloseAsync();
        }
    }
}
