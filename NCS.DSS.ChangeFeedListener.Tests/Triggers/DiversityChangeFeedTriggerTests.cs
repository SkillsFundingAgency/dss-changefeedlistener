using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using NCS.DSS.ChangeFeedListener.Model;
using NCS.DSS.ChangeFeedListener.ServiceBus;

namespace NCS.DSS.ChangeFeedListener.Tests.Triggers
{
    public class DiversityChangeFeedTriggerTests
    {
        private Mock<ILogger<DiversityChangeFeedTrigger.DiversityChangeFeedTrigger>> _logger;        
        private Mock<IChangeFeedListenerServiceBusClient> _serviceBusClient;
        private DiversityChangeFeedTrigger.DiversityChangeFeedTrigger _diversityTrigger;
        private string _documentId = string.Empty;
        private List<JsonDocument> _documentsList = [];

        [SetUp]
        public void Setup()
        {
            _logger = new Mock<ILogger<DiversityChangeFeedTrigger.DiversityChangeFeedTrigger>>();            
            _serviceBusClient = new Mock<IChangeFeedListenerServiceBusClient>();
            _diversityTrigger = new DiversityChangeFeedTrigger.DiversityChangeFeedTrigger(_serviceBusClient.Object,  _logger.Object);
            _documentId = "1afa77fa-d2f5-455d-837a-35b271ad0ec4";
            var jsonDocument = $"{{\"id\": \"{_documentId}\", \"CustomerId\": \"259810ce-dc25-4028-ab45-000010c322c9\", \"Address1\": \"Adddress Line 1\",\"Address2\": \"Adddress Line 2\",\"Address3\": \"Adddress Line 3\",\"Address4\": \"Adddress Line 4\",\"Address5\": \"Adddress Line 5\",\"PostCode\": \"DD11DD\",\"AlternativePostCode\": \"CC11CC\",\"Longitude\": -2.97227,\"Latitude\": 56.46236,\"EffectiveFrom\": \"2018-06-19T09:01:00Z\",\"EffectiveTo\": \"2018-06-21T13:12:00Z\",\"LastModifiedDate\": \"2018-06-21T13:45:00Z\",\"LastModifiedTouchpointId\": \"9999999999\",\"SubcontractorId\": \"\",\"CreatedBy\": \"9999999999\",\"_rid\": \"6jwfAMrhsgLyjAAAAAAAAA==\",\"_self\": \"dbs/6jwfAA==/colls/6jwfAMrhsgI=/docs/6jwfAMrhsgLyjAAAAAAAAA==/\",\"_etag\": \"\\\"3800d72c-0000-0d00-0000-66bf0ba30000\\\"\",\"_attachments\": \"attachments/\",\"_ts\": 1723796387}}";
            _documentsList =
                [
                     JsonSerializer.Deserialize<JsonDocument>(jsonDocument)
                ];
        }

        [Test]
        public async Task Run_CallsServiceBusClientMethodWithModelPropertyIsDiversityAsTrue_WhenDocumentIsValid()
        {
            //Arrange
            _serviceBusClient.Setup(s => s.SendChangeFeedMessageAsync(It.IsAny<string>(), It.IsAny<ChangeFeedMessageModel>())).Verifiable();

            //Act
            await _diversityTrigger.Run(_documentsList.AsReadOnly());

            //Assert
            _serviceBusClient.Verify(s => s.SendChangeFeedMessageAsync(It.IsAny<string>(), It.Is<ChangeFeedMessageModel>(m => m.IsDiversity == true)));
        }

        [Test]
        public async Task Run_LogsInformation_WhenDocumentIsValid()
        {
            //Arrange            
            var logMessage = string.Format("Attempting to send document id: {0} to service bus queue",_documentId);

            //Act
            await _diversityTrigger.Run(_documentsList.AsReadOnly());

            //Assert
            _logger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((x, _) => LogHelper.LogMessageMatcher(x, logMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Test]
        public async Task Run_LogsException_WhenServiceBusClientThrowsException()
        {
            //Arrange
            var exception = new Exception();
            var logMessage = "Error when trying to send message to service bus queue";

            _serviceBusClient.Setup(s => s.SendChangeFeedMessageAsync(It.IsAny<string>(), It.IsAny<ChangeFeedMessageModel>()))
                .Throws(exception);

            //Act
            await _diversityTrigger.Run(_documentsList.AsReadOnly());

            //Assert
            _logger.Verify(l => l.Log(
               LogLevel.Error,
               It.IsAny<EventId>(),
               It.Is<It.IsAnyType>((x, _) => LogHelper.LogMessageMatcher(x, logMessage)),
               exception,
               It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
               Times.Once);
        }
    }
}
