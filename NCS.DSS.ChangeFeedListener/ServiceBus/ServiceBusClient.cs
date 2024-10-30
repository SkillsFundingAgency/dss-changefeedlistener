using Microsoft.Azure.Documents;
using Microsoft.Azure.ServiceBus;
using NCS.DSS.ChangeFeedListener.Model;
using Newtonsoft.Json;
using System.Text;

namespace NCS.DSS.ChangeFeedListener.ServiceBus
{
    public class ServiceBusClient : IServiceBusClient
    {
        private readonly string _changeFeedQueueName;
        private readonly string _serviceBusConnectionString;

        public ServiceBusClient()
        {
            _changeFeedQueueName = Environment.GetEnvironmentVariable("ChangeFeedQueueName");
            _serviceBusConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
        }

        public async Task SendChangeFeedMessageAsync(Document document, ChangeFeedMessageModel changeFeedMessageModel)
        {
            if (_changeFeedQueueName == null)
                throw new ArgumentNullException(nameof(_changeFeedQueueName));

            if (_serviceBusConnectionString == null)
                throw new ArgumentNullException(nameof(_serviceBusConnectionString));

            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (changeFeedMessageModel == null)
                throw new ArgumentNullException(nameof(changeFeedMessageModel));

            var queueClient = new QueueClient(_serviceBusConnectionString, _changeFeedQueueName);

            var msg = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(changeFeedMessageModel)))
            {
                ContentType = "application/json",
                MessageId = document.Id + " " + DateTime.UtcNow
            };

            await queueClient.SendAsync(msg);
        }

    }
}