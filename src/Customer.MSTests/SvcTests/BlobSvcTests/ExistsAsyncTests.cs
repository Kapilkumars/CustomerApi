using Azure;
using Azure.Storage.Blobs;
using CustomerCustomerApi.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace CustomerCustomer.MSTests.SvcTests.BlobSvcTests
{
    [TestClass]
    public class ExistsAsyncTests
    {
        private Mock<BlobClient> _blobClientMock;
        private Mock<BlobContainerClient> _primaryContainerMock;
        private Mock<BlobContainerClient> _backupContainerMock;
        private Mock<ILogger<BlobSvc>> _loggerMock;
        private BlobSvc _blobSvc;

        [TestInitialize]
        public void Setup()
        {
            _blobClientMock = new Mock<BlobClient>();
            _primaryContainerMock = new Mock<BlobContainerClient>();
            _backupContainerMock = new Mock<BlobContainerClient>();
            _loggerMock = new Mock<ILogger<BlobSvc>>();

            var options = new BlobSvcOptions(
                "primary",
                "UseDevelopmentStorage=true",
                "backup"
            );

            // Setup container mocks to return the blob client
            _primaryContainerMock
                .Setup(c => c.GetBlobClient(It.IsAny<string>()))
                .Returns(_blobClientMock.Object);

            // Create service with mocked dependencies
            _blobSvc = new BlobSvc(
                options,
                _loggerMock.Object,
                _primaryContainerMock.Object,
                _backupContainerMock.Object
            );
        }

        [TestMethod]
        public async Task ExistsAsync_ReturnsTrue_WhenBlobExists()
        {
            // Arrange
            var filePath = "files/existing-file.txt";

            _blobClientMock
                .Setup(b => b.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(true, Mock.Of<Response>()));

            // Act
            var result = await _blobSvc.ExistsAsync(filePath);

            // Assert
            Assert.IsTrue(result);
            _primaryContainerMock.Verify(c => c.GetBlobClient(filePath), Times.Once);
            _blobClientMock.Verify(b => b.ExistsAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task ExistsAsync_ReturnsFalse_WhenBlobDoesNotExist()
        {
            // Arrange
            var filePath = "files/non-existing-file.txt";

            _blobClientMock
                .Setup(b => b.ExistsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(false, Mock.Of<Response>()));

            // Act
            var result = await _blobSvc.ExistsAsync(filePath);

            // Assert
            Assert.IsFalse(result);
            _primaryContainerMock.Verify(c => c.GetBlobClient(filePath), Times.Once);
            _blobClientMock.Verify(b => b.ExistsAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task ExistsAsync_PropagatesExceptions_WhenExistsThrows()
        {
            // Arrange
            var filePath = "files/error-file.txt";
            var errorMessage = "Storage operation failed";

            _blobClientMock
                .Setup(b => b.ExistsAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException(errorMessage));

            // Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<RequestFailedException>(() =>
                _blobSvc.ExistsAsync(filePath));

            Assert.AreEqual(errorMessage, exception.Message);
            _primaryContainerMock.Verify(c => c.GetBlobClient(filePath), Times.Once);
            _blobClientMock.Verify(b => b.ExistsAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
