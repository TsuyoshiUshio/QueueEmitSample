using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.Azure.ServiceBus;
using System.Configuration;
using Microsoft.ServiceBus.Messaging;
using System.Threading;

namespace QueueEmitSample
{
    class ServiceBusQueueSample
    {
        /// <summary>
        /// Sample for create a queue and send message. 
        /// https://blogs.msdn.microsoft.com/brunoterkaly/2014/08/07/learn-how-to-create-a-queue-place-and-read-a-message-using-azure-service-bus-queues-in-5-minutes/
        /// https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dotnet-get-started-with-queues
        /// For creating new queue and use namespace manager, we need WidowsAzure.SerivceBus package. for others Microsoft.Azure.ServiceBus.
        /// </summary>
        /// <returns></returns>
        public async Task ExecAsync()
        {
            // create a new queue
            const string SERVICE_BUS_CONNECTIONSTRING = "ServiceBus:ConnectionString";
            var namespaceManager = NamespaceManager.CreateFromConnectionString(ConfigurationManager.AppSettings.Get(SERVICE_BUS_CONNECTIONSTRING));
            var guid = Guid.NewGuid();
            var queueName = $"testosterone{guid}";
            if (await namespaceManager.QueueExistsAsync(queueName))
            {
                await namespaceManager.DeleteQueueAsync(queueName);
            }
            await namespaceManager.CreateQueueAsync(queueName);

            // send message

            // Note. QueueClient is resiside not only Microsoft.Azure.ServiceBus. Also Microsoft.ServiceBus
            queueClient = new Microsoft.Azure.ServiceBus.QueueClient(ConfigurationManager.AppSettings.Get(SERVICE_BUS_CONNECTIONSTRING), queueName);
            var message = new Message(Encoding.UTF8.GetBytes("hello world from service bus"));
            await queueClient.SendAsync(message);

            Console.WriteLine($"We create a queue named {queueName} and send message to ServiceBus. Please check it out");
            Console.ReadLine();

            // peek queue
            // Configure the MessageHandler Options in terms of exception handling, number of concurrent messages to deliver etc.
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHander)
            {
                // Maximum number of Concurrent calls to the callback `ProcessMessagesAsync`, set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
                // False below indicates the Complete will be handled by the User Callback as in `ProcessMessagesAsync` below.
                AutoComplete = false

            };
            queueClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);

            // Wait for callback
            Console.ReadKey();

            // Close QueueClient
            await queueClient.CloseAsync();

            // Delete the Queue
            await namespaceManager.DeleteQueueAsync(queueName);
        }

        async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            // Process the message
            Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");

            // Complete the message so that it is not received again.
            // This can be done only if the queueClient is created in ReceiveMode.PeekLock mode (which is default).
            await queueClient.CompleteAsync(message.SystemProperties.LockToken);

            // Note: Use the cancellationToken passed as necessary to determine if the queueClient has already been closed.
            // If queueClient has already been Closed, you may chose to not call CompleteAsync() or AbandonAsync() etc. calls 
            // to avoid unnecessary exceptions.
        }

        Task ExceptionReceivedHander(Microsoft.Azure.ServiceBus.ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }
        private Microsoft.Azure.ServiceBus.QueueClient queueClient;
    }
}