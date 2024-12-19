using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.ChangeFeedListener.Model;
using Newtonsoft.Json;
using System.Text;

namespace NCS.DSS.ChangeFeedListener.ServiceBus
{
    public class ChangeFeedListenerServiceBusClient : IChangeFeedListenerServiceBusClient
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ILogger<ChangeFeedListenerServiceBusClient> _logger;
        private readonly string _queueName;

        public ChangeFeedListenerServiceBusClient(ServiceBusClient serviceBusClient,
            IOptions<ChangeFeedListenerConfigurationSettings> configOptions,
            ILogger<ChangeFeedListenerServiceBusClient> logger)
        {
            var queueName = configOptions.Value.ChangeFeedQueueName;
            if (string.IsNullOrEmpty(queueName))
            {
                throw new ArgumentNullException(nameof(queueName), "QueueName cannot be null or empty.");
            }

            _serviceBusClient = serviceBusClient;
            _queueName = queueName;
            _logger = logger;
        }

        public async Task SendChangeFeedMessageAsync(string documentId, ChangeFeedMessageModel changeFeedMessageModel)
        {
            var serviceBusSender = _serviceBusClient.CreateSender(_queueName);

            if (documentId == null)
                throw new ArgumentNullException(nameof(documentId));

            if (changeFeedMessageModel == null)
                throw new ArgumentNullException(nameof(changeFeedMessageModel));

            var msg = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(changeFeedMessageModel)))
            {
                ContentType = "application/json",
                MessageId = documentId + " " + DateTime.UtcNow
            };

            await serviceBusSender.SendMessageAsync(msg);
        }
    }
}