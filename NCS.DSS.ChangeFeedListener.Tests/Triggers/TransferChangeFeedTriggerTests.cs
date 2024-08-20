using Moq;
using Microsoft.Extensions.Logging;
using NCS.DSS.ChangeFeedListener.ServiceBus;
using DFC.Common.Standard.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Documents;
using NCS.DSS.ChangeFeedListener.Model;

namespace NCS.DSS.ChangeFeedListener.Tests.Triggers
{
    public class TransferChangeFeedTriggerTests
    {
        private Mock<ILogger<TransferChangeFeedTrigger.TransferChangeFeedTrigger>> _logger;
        private Mock<ILoggerHelper> _loggerHelper;
        private Mock<IServiceBusClient> _serviceBusClient;
        private TransferChangeFeedTrigger.TransferChangeFeedTrigger _transferTrigger;
        private string _jsonDocument = string.Empty;

        [SetUp]
        public void Setup()
        {
            _logger = new Mock<ILogger<TransferChangeFeedTrigger.TransferChangeFeedTrigger>>();
            _loggerHelper = new Mock<ILoggerHelper>();
            _serviceBusClient = new Mock<IServiceBusClient>();
            _transferTrigger = new TransferChangeFeedTrigger.TransferChangeFeedTrigger(_serviceBusClient.Object, _loggerHelper.Object, _logger.Object);
            var documentId = "1afa77fa-d2f5-455d-837a-35b271ad0ec4";
            _jsonDocument = $"{{\"id\": \"{documentId}\", \"CustomerId\": \"259810ce-dc25-4028-ab45-000010c322c9\", \"Address1\": \"Adddress Line 1\",\"Address2\": \"Adddress Line 2\",\"Address3\": \"Adddress Line 3\",\"Address4\": \"Adddress Line 4\",\"Address5\": \"Adddress Line 5\",\"PostCode\": \"DD11DD\",\"AlternativePostCode\": \"CC11CC\",\"Longitude\": -2.97227,\"Latitude\": 56.46236,\"EffectiveFrom\": \"2018-06-19T09:01:00Z\",\"EffectiveTo\": \"2018-06-21T13:12:00Z\",\"LastModifiedDate\": \"2018-06-21T13:45:00Z\",\"LastModifiedTouchpointId\": \"9999999999\",\"SubcontractorId\": \"\",\"CreatedBy\": \"9999999999\",\"_rid\": \"6jwfAMrhsgLyjAAAAAAAAA==\",\"_self\": \"dbs/6jwfAA==/colls/6jwfAMrhsgI=/docs/6jwfAMrhsgLyjAAAAAAAAA==/\",\"_etag\": \"\\\"3800d72c-0000-0d00-0000-66bf0ba30000\\\"\",\"_attachments\": \"attachments/\",\"_ts\": 1723796387}}";
        }        

        [Test]
        public async Task Run_CallsServiceBusClientMethodWithModelPropertyIsTransferAsTrue_WhenDocumentIsValid()
        {
            //Arrange
            var document = JsonConvert.DeserializeObject<Document>(_jsonDocument);

            var documentsList = new List<Document>
            {
                document
            };

            _serviceBusClient.Setup(s => s.SendChangeFeedMessageAsync(It.IsAny<Document>(), It.IsAny<ChangeFeedMessageModel>())).Verifiable();


            //Act
            await _transferTrigger.Run(documentsList.AsReadOnly());

            //Assert
            _serviceBusClient.Verify(s => s.SendChangeFeedMessageAsync(It.IsAny<Document>(), It.Is<ChangeFeedMessageModel>(m => m.IsTransfer == true)));
        }

        [Test]
        public async Task Run_LogsInformation_WhenDocumentIsValid()
        {
            //Arrange
            var document = JsonConvert.DeserializeObject<Document>(_jsonDocument);
            var logMessage = string.Format("Attempting to send document id: {0} to service bus queue", document.Id);

            var documentsList = new List<Document>
            {
                document
            };

            _loggerHelper.Setup(l => l.LogInformationMessage(_logger.Object, It.IsAny<Guid>(), It.IsAny<string>())).Verifiable();

            //Act
            await _transferTrigger.Run(documentsList.AsReadOnly());

            //Assert
            _loggerHelper.Verify(l => l.LogInformationMessage(_logger.Object, It.IsAny<Guid>(), logMessage), Times.Once);
        }

        [Test]
        public async Task Run_LogsException_WhenServiceBusClientThrowsException()
        {
            //Arrange
            var document = JsonConvert.DeserializeObject<Document>(_jsonDocument);
            var exception = new Exception();
            var logMessage = "Error when trying to send message to service bus queue";

            var documentsList = new List<Document>
            {
                document
            };

            _serviceBusClient.Setup(s => s.SendChangeFeedMessageAsync(It.IsAny<Document>(), It.IsAny<ChangeFeedMessageModel>()))
                .Throws(exception);

            //Act
            await _transferTrigger.Run(documentsList.AsReadOnly());

            //Assert
            _loggerHelper.Verify(l => l.LogException(_logger.Object, It.IsAny<Guid>(), logMessage, exception), Times.Once);            
        }        
    }
}
