using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CustomerCustomerApi.Exceptions;
using CustomerCustomerApi.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;

namespace CustomerCustomer.MSTests.SvcTests.BlobSvcTests
{
    // Minimal BlobSvc stub for testing with injected container client
    public class TestBlobSvc
    {
        private readonly BlobContainerClient _containerClient;
        private readonly ILogger<BlobSvc> _logger;

        public TestBlobSvc(BlobContainerClient containerClient, ILogger<BlobSvc> logger)
        {
            _containerClient = containerClient;
            _logger = logger;
        }

        // Mimic the Upload method we have in BlobSvc
        public async Task<string> Upload(string blobName, BinaryData data, bool overwrite, IDictionary<string, string>? tags = null)
        {
            try
            {
                var blobClient = _containerClient.GetBlobClient(blobName);

                await blobClient.UploadAsync(data, overwrite, CancellationToken.None);

                if (tags != null)
                {
                    await blobClient.SetTagsAsync(tags, conditions: null, CancellationToken.None);
                }

                return blobClient.Uri.AbsoluteUri;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Could not upload the file: {blobName}");
                throw new BlobSvcException(_logger, $"Could not upload the file: {blobName}", ex);
            }
        }
    }

    [TestClass]
    public class UploadTests
    {
        private Mock<ILogger<BlobSvc>> _loggerMock = null!;
        private Mock<BlobContainerClient> _containerClientMock = null!;
        private Mock<BlobClient> _blobClientMock = null!;
        private TestBlobSvc _blobSvc = null!;

        [TestInitialize]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<BlobSvc>>();
            _containerClientMock = new Mock<BlobContainerClient>();
            _blobClientMock = new Mock<BlobClient>();

            // Setup GetBlobClient to return mocked blob client
            _containerClientMock
                .Setup(c => c.GetBlobClient(It.IsAny<string>()))
                .Returns(_blobClientMock.Object);

            // Instantiate test blob service with mocks
            _blobSvc = new TestBlobSvc(_containerClientMock.Object, _loggerMock.Object);
        }

        [TestMethod]
        public async Task Upload_ReturnsUri_WhenUploadSucceeds()
        {
            // Arrange
            var blobName = "testfile.txt";
            var data = new BinaryData(Encoding.UTF8.GetBytes("Test content"));
            var overwrite = true;
            var expectedUri = new Uri("https://fake.blob.core.windows.net/container/testfile.txt");

            _blobClientMock
                .Setup(b => b.UploadAsync(data, overwrite, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<Response<BlobContentInfo>>());

            _blobClientMock
                .Setup(b => b.Uri)
                .Returns(expectedUri);

            // Act
            var result = await _blobSvc.Upload(blobName, data, overwrite);

            // Assert
            Assert.AreEqual(expectedUri.AbsoluteUri, result);
            _blobClientMock.Verify(b => b.UploadAsync(data, overwrite, It.IsAny<CancellationToken>()), Times.Once);
            _blobClientMock.Verify(b => b.SetTagsAsync(It.IsAny<IDictionary<string, string>>(), null, It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestMethod]
        public async Task Upload_SetsTags_WhenTagsProvided()
        {
            // Arrange
            var blobName = "with-tags.json";
            var data = new BinaryData("Test with tags");
            var overwrite = false;
            var tags = new Dictionary<string, string> { { "env", "dev" } };
            var expectedUri = new Uri("https://fake.blob.core.windows.net/container/with-tags.json");

            _blobClientMock
                .Setup(b => b.UploadAsync(data, overwrite, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<Response<BlobContentInfo>>());

            _blobClientMock
                .Setup(b => b.SetTagsAsync(tags, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<Response>());

            _blobClientMock
                .Setup(b => b.Uri)
                .Returns(expectedUri);

            // Act
            var result = await _blobSvc.Upload(blobName, data, overwrite, tags);

            // Assert
            Assert.AreEqual(expectedUri.AbsoluteUri, result);
            _blobClientMock.Verify(b => b.UploadAsync(data, overwrite, It.IsAny<CancellationToken>()), Times.Once);
            _blobClientMock.Verify(b => b.SetTagsAsync(tags, null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task Upload_ThrowsBlobSvcException_WhenUploadFails()
        {
            // Arrange
            var blobName = "failupload.docx";
            var data = new BinaryData("Some binary content");

            _blobClientMock
                .Setup(b => b.UploadAsync(data, It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Upload failed"));

            // Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<BlobSvcException>(() =>
                _blobSvc.Upload(blobName, data, overwrite: true));

            Assert.IsTrue(exception.Message.Contains($"Could not upload the file: {blobName}"));
            _loggerMock.Verify(
                logger => logger.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2)); // Expecting 2 logs because BlobSvcException logs internally

        }
    }
}