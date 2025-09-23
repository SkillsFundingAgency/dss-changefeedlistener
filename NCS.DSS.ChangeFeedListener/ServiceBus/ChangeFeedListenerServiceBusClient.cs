using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.ChangeFeedListener.Model;
using System.Text;
using System.Text.Json;

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
            try
            {
                _logger.LogTrace("Attempting to Create Sender for Service Bus Queue {QueueName}", _queueName);
                var serviceBusSender = _serviceBusClient.CreateSender(_queueName);

                if (documentId == null)
                {
                    var ex = new ArgumentNullException(nameof(documentId));
                    _logger.LogError(ex, "Failed to Send Message to Service Bus. Document ID is null");
                    throw ex;
                }

                if (changeFeedMessageModel == null)
                {
                    var ex = new ArgumentNullException(nameof(changeFeedMessageModel));
                    _logger.LogError(ex, "Failed to Send Message to Service Bus. ChangeFeedMessageModel is null");
                    throw ex;
                }

                _logger.LogTrace("Attempting to Create Service Bus Message for Document ID {DocumentID}", documentId);
                var msg = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(changeFeedMessageModel)))
                {
                    ContentType = "application/json",
                    MessageId = documentId + " " + DateTime.UtcNow
                };
                _logger.LogTrace("Attempting to Send Service Bus Message for Document ID {DocumentID}", documentId);
                await serviceBusSender.SendMessageAsync(msg);
                _logger.LogTrace("Successfully Sent Service Bus Message for Document ID {DocumentID}", documentId);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to Send Message to Service Bus. Exception Raised with Message {Exception}",ex.Message);
                throw;
            }
           
        }
    }
}