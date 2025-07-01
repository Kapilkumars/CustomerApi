using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CustomerCustomerApi.Services;
using Microsoft.Extensions.Logging;
using Moq;
namespace CustomerCustomer.MSTests.SvcTests.BlobSvcTests
{

    [TestClass]
    public class GetTagsTests
    {
        private Mock<BlobClient> _blobClientMock;
        private Mock<BlobContainerClient> _containerMock;
        private Mock<ILogger<BlobSvc>> _loggerMock;
        private BlobSvc _blobSvc;

        [TestInitialize]
        public void Setup()
        {
            _blobClientMock = new Mock<BlobClient>();
            _containerMock = new Mock<BlobContainerClient>();
            _loggerMock = new Mock<ILogger<BlobSvc>>();

            var options = new BlobSvcOptions(
                "primary",
                 "UseDevelopmentStorage=true",
                 "backup"
             );


            _containerMock.Setup(c => c.GetBlobClient(It.IsAny<string>())).Returns(_blobClientMock.Object);
            _blobSvc = new BlobSvc(options, _loggerMock.Object, _containerMock.Object, _containerMock.Object);
        }

        [TestMethod]
        public async Task GetTags_ReturnsTags_WhenTagsExist()
        {
            // Arrange
            var blobName = "sample-blob";
            var expectedTags = new Dictionary<string, string> { { "env", "test" } };

            var mockTagResult = BlobsModelFactory.GetBlobTagResult(tags: expectedTags);

            var mockResponse = Mock.Of<Response<GetBlobTagResult>>(r =>
                r.Value == mockTagResult
            );

            _blobClientMock.Setup(b => b.GetTagsAsync(default, It.IsAny<CancellationToken>()))
                           .ReturnsAsync(mockResponse);

            // Act
            var result = await _blobSvc.GetTags(blobName);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("test", result["env"]);
        }


        [TestMethod]
        public async Task GetTags_ReturnsNull_WhenExceptionOccurs()
        {
            // Arrange
            var blobName = "invalid-blob";
            _blobClientMock.Setup(b => b.GetTagsAsync(default, It.IsAny<CancellationToken>()))
                           .ThrowsAsync(new RequestFailedException("Blob not found"));

            // Act
            var result = await _blobSvc.GetTags(blobName);

            // Assert
            Assert.IsNull(result);
            _loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Could not retrieve the tags for blob")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}