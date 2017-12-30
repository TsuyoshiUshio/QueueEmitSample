using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage;
using System.Configuration;

namespace QueueEmitSample
{
    class StorageQueueSample
    {
        /// <summary>
        /// Storage Queue sample progam
        /// https://docs.microsoft.com/en-us/azure/storage/queues/storage-dotnet-how-to-use-queues
        /// </summary>
        /// <returns></returns>
        public async Task ExecuteAsync()
        {
            // Configuration
            var storageAccount = CloudStorageAccount.Parse(
                ConfigurationManager.AppSettings.Get("Storage:ConnectionString"));
            var queueClient = storageAccount.CreateCloudQueueClient();
            // Create a new queue
            var guid = Guid.NewGuid();
            var queueName = $"testosterone{guid}";
            var queue = queueClient.GetQueueReference(queueName);
            await queue.CreateIfNotExistsAsync();

            // Emitt queue
            var message = new CloudQueueMessage("hello world");
           
            await queue.AddMessageAsync(message);
            // Prompt
            Console.WriteLine($"We create {queueName} please check it out");
            Console.ReadLine();
            
            // Peek at the next message
            var peekedMessage = await queue.PeekMessageAsync();
            Console.WriteLine($"Peeked message: {peekedMessage.AsString}");

            // Delete Queue
            await queue.DeleteAsync();
        }
    }
}
