using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CustomerCustomerApi.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace CustomerCustomer.MSTests.SvcTests.BlobSvcTests
{
    [TestClass]
    public class GetBackupTagsTests
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

            _backupContainerMock
                .Setup(c => c.GetBlobClient(It.IsAny<string>()))
                .Returns(_blobClientMock.Object);

            _blobSvc = new BlobSvc(
                options,
                _loggerMock.Object,
                _primaryContainerMock.Object,
                _backupContainerMock.Object
            );
        }

        [TestMethod]
        public async Task GetBackupTags_ReturnsTags_WhenTagsExist()
        {
            // Arrange
            var blobName = "backup-blob";
            var expectedTags = new Dictionary<string, string> { { "env", "test" }, { "version", "1.0" } };
            var mockTagResult = BlobsModelFactory.GetBlobTagResult(tags: expectedTags);
            var mockResponse = Mock.Of<Response<GetBlobTagResult>>(r =>
                r.Value == mockTagResult
            );

            _blobClientMock
                .Setup(b => b.GetTagsAsync(null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse);

            // Act
            var result = await _blobSvc.GetBackupTags(blobName);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("test", result["env"]);
            Assert.AreEqual("1.0", result["version"]);
            _backupContainerMock.Verify(c => c.GetBlobClient(blobName), Times.Once);
            _blobClientMock.Verify(b => b.GetTagsAsync(null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task GetBackupTags_ReturnsNull_WhenExceptionOccurs()
        {
            // Arrange
            var blobName = "non-existent-blob";

            _blobClientMock
                .Setup(b => b.GetTagsAsync(null, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RequestFailedException("Blob not found"));

            // Act
            var result = await _blobSvc.GetBackupTags(blobName);

            // Assert
            Assert.IsNull(result);
            _loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Could not retrieve the tags for blob {blobName}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [TestMethod]
        public async Task GetBackupTags_HandlesEmptyTags()
        {
            // Arrange
            var blobName = "empty-tags-blob";
            var emptyTags = new Dictionary<string, string>();
            var mockTagResult = BlobsModelFactory.GetBlobTagResult(tags: emptyTags);
            var mockResponse = Mock.Of<Response<GetBlobTagResult>>(r =>
                r.Value == mockTagResult
            );

            _blobClientMock
                .Setup(b => b.GetTagsAsync(null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockResponse);

            // Act
            var result = await _blobSvc.GetBackupTags(blobName);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task GetBackupTags_PassesCancellationToken()
        {
            // Arrange
            var blobName = "cancellation-test-blob";
            var expectedTags = new Dictionary<string, string> { { "test", "value" } };
            var mockTagResult = BlobsModelFactory.GetBlobTagResult(tags: expectedTags);
            var mockResponse = Mock.Of<Response<GetBlobTagResult>>(r =>
                r.Value == mockTagResult
            );

            var cancellationToken = new CancellationToken();

            _blobClientMock
                .Setup(b => b.GetTagsAsync(null, cancellationToken))
                .ReturnsAsync(mockResponse)
                .Verifiable();

            // Act
            var result = await _blobSvc.GetBackupTags(blobName, cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            _blobClientMock.Verify(b => b.GetTagsAsync(null, cancellationToken), Times.Once);
        }
    }
}
