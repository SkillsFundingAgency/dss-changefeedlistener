using Microsoft.Azure.Documents;
using NCS.DSS.ChangeFeedListener.Model;
using NCS.DSS.ChangeFeedListener.ServiceBus;

namespace NCS.DSS.ChangeFeedListener.Tests.ServiceBus
{
    public class ServiceBusClientTests
    {
        private const string ValidQueueName = "test-queue";
        private const string ValidConnectionString = "Endpoint=sb://test.servicebus.windows.net/;SharedAccessKeyName=TestKey;SharedAccessKey=TestAccessKey";

        public ServiceBusClientTests()
        {
            Environment.SetEnvironmentVariable("ChangeFeedQueueName", ValidQueueName);
            Environment.SetEnvironmentVariable("ServiceBusConnectionString", ValidConnectionString);
        }

        [Test]
        public void Constructor_ShouldSetEnvironmentVariables_WhenObjectIsInstantiated()
        {
            // Arrange & Act
            var serviceBusClient = new ServiceBusClient();

            // Assert
            Assert.That(ValidQueueName, Is.EqualTo(GetPrivateField<string>(serviceBusClient, "_changeFeedQueueName")));
            Assert.That(ValidConnectionString, Is.EqualTo(GetPrivateField<string>(serviceBusClient, "_serviceBusConnectionString")));
        }

        [Test]
        public void SendChangeFeedMessageAsync_ShouldThrowArgumentNullException_WhenDocumentIsNull()
        {
            // Arrange
            var serviceBusClient = new ServiceBusClient();
            var changeFeedMessageModel = new ChangeFeedMessageModel();

            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => serviceBusClient.SendChangeFeedMessageAsync(null, changeFeedMessageModel));
        }

        [Test]
        public void SendChangeFeedMessageAsync_ShouldThrowArgumentNullException_WhenChangeFeedMessageModelIsNull()
        {
            // Arrange
            var serviceBusClient = new ServiceBusClient();
            var document = new Document { Id = "test-id" };

            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => serviceBusClient.SendChangeFeedMessageAsync(document, null));
        }

        [Test]
        public void SendChangeFeedMessageAsync_ShouldThrowArgumentNullException_WhenQueueNameIsNull()
        {
            // Arrange
            Environment.SetEnvironmentVariable("ChangeFeedQueueName", null);
            var serviceBusClient = new ServiceBusClient();
            var document = new Document { Id = "test-id" };
            var changeFeedMessageModel = new ChangeFeedMessageModel();

            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => serviceBusClient.SendChangeFeedMessageAsync(document, changeFeedMessageModel));
        }

        [Test]
        public void SendChangeFeedMessageAsync_ShouldThrowArgumentNullException_WhenConnectionStringIsNull()
        {
            // Arrange
            Environment.SetEnvironmentVariable("ServiceBusConnectionString", null);
            var serviceBusClient = new ServiceBusClient();
            var document = new Document { Id = "test-id" };
            var changeFeedMessageModel = new ChangeFeedMessageModel();

            // Act & Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => serviceBusClient.SendChangeFeedMessageAsync(document, changeFeedMessageModel));
        }

        private T GetPrivateField<T>(object obj, string fieldName)
        {
            var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (T)field.GetValue(obj);
        }
    }
}
