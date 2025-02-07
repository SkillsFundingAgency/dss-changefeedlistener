using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NCS.DSS.ChangeFeedListener.Model;
using NCS.DSS.ChangeFeedListener.ServiceBus;

namespace NCS.DSS.ChangeFeedListener.Tests.ServiceBus
{
    public class ServiceBusClientTests
    {
        private ChangeFeedListenerServiceBusClient _cflServiceBusClient;
        private Mock<ServiceBusClient> _serviceBusClient;
        private Mock<ILogger<ChangeFeedListenerServiceBusClient>> _logger;
        private Mock<IOptions<ChangeFeedListenerConfigurationSettings>> _configOptions;
        private readonly string _queueName = "dss.changefeedqueue";
        [SetUp]
        public void Setup()
        {
            var configs = new ChangeFeedListenerConfigurationSettings
            {
                ChangeFeedQueueName = _queueName
            };
            _logger = new Mock<ILogger<ChangeFeedListenerServiceBusClient>>();
            _configOptions = new Mock<IOptions<ChangeFeedListenerConfigurationSettings>>();
            _configOptions.Setup(s => s.Value).Returns(configs);
            _serviceBusClient = new Mock<ServiceBusClient>();
            _cflServiceBusClient = new ChangeFeedListenerServiceBusClient(_serviceBusClient.Object, _configOptions.Object, _logger.Object);
        }

        [Test]
        public void SendChangeFeedMessageAsync_ShouldThrowArgumentNullException_WhenDocumentIsNull()
        {
            // Arrange
            var changeFeedMessageModel = new ChangeFeedMessageModel();

            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => _cflServiceBusClient.SendChangeFeedMessageAsync(null, changeFeedMessageModel));
        }

        [Test]
        public void SendChangeFeedMessageAsync_ShouldThrowArgumentNullException_WhenChangeFeedMessageModelIsNull()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => _cflServiceBusClient.SendChangeFeedMessageAsync("test-id", null));
        }       
    }
}
