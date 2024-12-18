using NCS.DSS.ChangeFeedListener.Model;
using NCS.DSS.ChangeFeedListener.ServiceBus;

namespace NCS.DSS.ChangeFeedListener.Tests.ServiceBus
{
    public class ServiceBusClientTests
    {
        private readonly IChangeFeedListenerServiceBusClient _serviceBusClient;
        public ServiceBusClientTests(IChangeFeedListenerServiceBusClient serviceBusClient)
        {
           _serviceBusClient = serviceBusClient;
        }

        [Test]
        public void SendChangeFeedMessageAsync_ShouldThrowArgumentNullException_WhenDocumentIsNull()
        {
            // Arrange
            
            var changeFeedMessageModel = new ChangeFeedMessageModel();

            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => _serviceBusClient.SendChangeFeedMessageAsync(null, changeFeedMessageModel));
        }

        [Test]
        public void SendChangeFeedMessageAsync_ShouldThrowArgumentNullException_WhenChangeFeedMessageModelIsNull()
        {
            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => _serviceBusClient.SendChangeFeedMessageAsync("test-id", null));
        }

        [Test]
        public void SendChangeFeedMessageAsync_ShouldThrowArgumentNullException_WhenQueueNameIsNull()
        {
            // Arrange
            Environment.SetEnvironmentVariable("ChangeFeedQueueName", null);
            
            var changeFeedMessageModel = new ChangeFeedMessageModel();

            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => _serviceBusClient.SendChangeFeedMessageAsync("test-id", changeFeedMessageModel));
        }

        [Test]
        public void SendChangeFeedMessageAsync_ShouldThrowArgumentNullException_WhenConnectionStringIsNull()
        {
            // Arrange
            Environment.SetEnvironmentVariable("ServiceBusConnectionString", null);
            
            var changeFeedMessageModel = new ChangeFeedMessageModel();

            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => _serviceBusClient.SendChangeFeedMessageAsync("test-id", changeFeedMessageModel));
        }
    }
}
