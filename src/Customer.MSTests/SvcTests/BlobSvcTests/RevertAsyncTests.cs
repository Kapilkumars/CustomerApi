using Azure;

using Azure.Storage.Blobs;

using Azure.Storage.Blobs.Models;

using CustomerCustomerApi.Exceptions;

using CustomerCustomerApi.Services;

using Microsoft.Extensions.Logging;

using Moq;

namespace CustomerCustomer.MSTests.SvcTests.BlobSvcTests

{

    [TestClass]

    public class RevertAsyncTests

    {

        private Mock<BlobClient> _sourceBlobMock;

        private Mock<BlobClient> _destinationBlobMock;

        private Mock<BlobContainerClient> _primaryContainerMock;

        private Mock<BlobContainerClient> _backupContainerMock;

        private Mock<ILogger<BlobSvc>> _loggerMock;

        private BlobSvc _blobSvc;

        private Uri _destinationUri;

        [TestInitialize]

        public void Setup()

        {

            _sourceBlobMock = new Mock<BlobClient>();

            _destinationBlobMock = new Mock<BlobClient>();

            _primaryContainerMock = new Mock<BlobContainerClient>();

            _backupContainerMock = new Mock<BlobContainerClient>();

            _loggerMock = new Mock<ILogger<BlobSvc>>();

            _destinationUri = new Uri("https://test.blob.core.windows.net/container/destination-blob");

            var options = new BlobSvcOptions(

                "primary",

                "UseDevelopmentStorage=true",

                "backup"

            );

            // Setup destination blob mock

            _destinationBlobMock.Setup(b => b.Uri).Returns(_destinationUri);

            // Setup container mocks to return the appropriate blob clients

            _primaryContainerMock

                .Setup(c => c.GetBlobClient(It.IsAny<string>()))

                .Returns(_sourceBlobMock.Object);

            _backupContainerMock

                .Setup(c => c.GetBlobClient(It.IsAny<string>()))

                .Returns(_destinationBlobMock.Object);

            // Create service with mocked dependencies

            _blobSvc = new BlobSvc(

                options,

                _loggerMock.Object,

                _primaryContainerMock.Object,

                _backupContainerMock.Object

            );

        }

        [TestMethod]

        public async Task RevertAsync_CopiesFileWithTags_WhenSuccessful()

        {

            // Arrange

            var sourcePath = "primary/file.txt";

            var destinationPath = "backup/file.txt";

            var cancellationToken = new CancellationToken();

            var existingTags = new Dictionary<string, string> { { "spaceId", "space123" }, { "version", "1.0" } };

            var mockTagResult = BlobsModelFactory.GetBlobTagResult(tags: existingTags);

            var mockTagResponse = Mock.Of<Response<GetBlobTagResult>>(r => r.Value == mockTagResult);

            _destinationBlobMock

                .Setup(b => b.GetTagsAsync(It.IsAny<BlobRequestConditions>(), cancellationToken))

                .ReturnsAsync(mockTagResponse)

                .Verifiable();

            var mockCopyOperation = new Mock<CopyFromUriOperation>();

            var mockCompletionResponse = Mock.Of<Response<long>>(r => r.Value == 1L);

            mockCopyOperation

                .Setup(o => o.WaitForCompletionAsync(It.IsAny<CancellationToken>()))

                .Returns(new ValueTask<Response<long>>(mockCompletionResponse));

            _sourceBlobMock

                .Setup(b => b.StartCopyFromUriAsync(It.IsAny<Uri>(), It.IsAny<BlobCopyFromUriOptions>(), cancellationToken))

                .ReturnsAsync(mockCopyOperation.Object)

                .Verifiable();

            // Act

            await _blobSvc.RevertAsync(sourcePath, destinationPath, cancellationToken);

            // Assert

            _destinationBlobMock.Verify();

            _sourceBlobMock.Verify();

            mockCopyOperation.Verify(o => o.WaitForCompletionAsync(cancellationToken), Times.Once);

        }

        [TestMethod]

        public async Task RevertAsync_UsesEmptyTags_WhenGetTagsReturnsNull()

        {

            // Arrange

            var sourcePath = "primary/file.txt";

            var destinationPath = "backup/file.txt";

            var cancellationToken = new CancellationToken();

            // Mock GetBackupTags to return null, simulating no tags

            _backupContainerMock

                .Setup(c => c.GetBlobClient(sourcePath))

                .Returns(_destinationBlobMock.Object);

            Response<GetBlobTagResult> nullTagResponse = null!;

            _destinationBlobMock

                .Setup(b => b.GetTagsAsync(It.IsAny<BlobRequestConditions>(), cancellationToken))

                .ReturnsAsync(nullTagResponse)

                .Verifiable();

            var mockCopyOperation = new Mock<CopyFromUriOperation>();

            var mockCompletionResponse = Mock.Of<Response<long>>(r => r.Value == 1L);

            mockCopyOperation

                .Setup(o => o.WaitForCompletionAsync(It.IsAny<CancellationToken>()))

                .Returns(new ValueTask<Response<long>>(mockCompletionResponse));

            _sourceBlobMock

                .Setup(b => b.StartCopyFromUriAsync(It.IsAny<Uri>(), It.Is<BlobCopyFromUriOptions>(opt =>

                    opt.Tags != null && opt.Tags.Count == 0), cancellationToken))

                .ReturnsAsync(mockCopyOperation.Object)

                .Verifiable();

            // Act

            await _blobSvc.RevertAsync(sourcePath, destinationPath, cancellationToken);

            // Assert

            _destinationBlobMock.Verify();

            _sourceBlobMock.Verify();

            mockCopyOperation.Verify(o => o.WaitForCompletionAsync(cancellationToken), Times.Once);

        }

        [TestMethod]

        public async Task RevertAsync_PassesCancellationToken_ToAllOperations()

        {

            // Arrange

            var sourcePath = "primary/file.txt";

            var destinationPath = "backup/file.txt";

            var cancellationToken = new CancellationToken();

            var existingTags = new Dictionary<string, string> { { "key", "value" } };

            var mockTagResult = BlobsModelFactory.GetBlobTagResult(tags: existingTags);

            var mockTagResponse = Mock.Of<Response<GetBlobTagResult>>(r => r.Value == mockTagResult);

            _destinationBlobMock

                .Setup(b => b.GetTagsAsync(It.IsAny<BlobRequestConditions>(), cancellationToken))

                .ReturnsAsync(mockTagResponse)

                .Verifiable();

            var mockCopyOperation = new Mock<CopyFromUriOperation>();

            var mockCompletionResponse = Mock.Of<Response<long>>(r => r.Value == 1L);

            mockCopyOperation

                .Setup(o => o.WaitForCompletionAsync(cancellationToken))

                .Returns(new ValueTask<Response<long>>(mockCompletionResponse));

            _sourceBlobMock

                .Setup(b => b.StartCopyFromUriAsync(It.IsAny<Uri>(), It.IsAny<BlobCopyFromUriOptions>(), cancellationToken))

                .ReturnsAsync(mockCopyOperation.Object)

                .Verifiable();

            // Act

            await _blobSvc.RevertAsync(sourcePath, destinationPath, cancellationToken);

            // Assert

            _destinationBlobMock.Verify(b => b.GetTagsAsync(It.IsAny<BlobRequestConditions>(), cancellationToken), Times.Once);

            _sourceBlobMock.Verify(b => b.StartCopyFromUriAsync(It.IsAny<Uri>(), It.IsAny<BlobCopyFromUriOptions>(), cancellationToken), Times.Once);

            mockCopyOperation.Verify(o => o.WaitForCompletionAsync(cancellationToken), Times.Once);

        }

        [TestMethod]

        public async Task RevertAsync_ThrowsBlobSvcException_WhenCopyFails()

        {

            // Arrange

            var sourcePath = "primary/error-file.txt";

            var destinationPath = "backup/error-file.txt";

            var errorMessage = "Copy operation failed";

            var existingTags = new Dictionary<string, string> { { "key", "value" } };

            var mockTagResult = BlobsModelFactory.GetBlobTagResult(tags: existingTags);

            var mockTagResponse = Mock.Of<Response<GetBlobTagResult>>(r => r.Value == mockTagResult);

            _destinationBlobMock

                .Setup(b => b.GetTagsAsync(It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))

                .ReturnsAsync(mockTagResponse);

            _sourceBlobMock

                .Setup(b => b.StartCopyFromUriAsync(

                    It.IsAny<Uri>(),

                    It.IsAny<BlobCopyFromUriOptions>(),

                    It.IsAny<CancellationToken>()))

                .ThrowsAsync(new Exception(errorMessage));

            // Act & Assert

            var exception = await Assert.ThrowsExceptionAsync<BlobSvcException>(() =>

                _blobSvc.RevertAsync(sourcePath, destinationPath));

            Assert.IsTrue(exception.Message.Contains("An unexpected error occurred"));

            Assert.IsTrue(exception.InnerException.Message.Contains(errorMessage));

        }

        [TestMethod]

        public async Task RevertAsync_ThrowsBlobSvcException_WhenOperationCanceled()

        {

            // Arrange

            var sourcePath = "primary/canceled-file.txt";

            var destinationPath = "backup/canceled-file.txt";

            var cancelMessage = "Operation was canceled by user";

            var existingTags = new Dictionary<string, string> { { "key", "value" } };

            var mockTagResult = BlobsModelFactory.GetBlobTagResult(tags: existingTags);

            var mockTagResponse = Mock.Of<Response<GetBlobTagResult>>(r => r.Value == mockTagResult);

            _destinationBlobMock

                .Setup(b => b.GetTagsAsync(It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))

                .ReturnsAsync(mockTagResponse);

            // Setup copy operation to throw OperationCanceledException

            _sourceBlobMock

                .Setup(b => b.StartCopyFromUriAsync(

                    It.IsAny<Uri>(),

                    It.IsAny<BlobCopyFromUriOptions>(),

                    It.IsAny<CancellationToken>()))

                .ThrowsAsync(new OperationCanceledException(cancelMessage));

            // Act & Assert

            var exception = await Assert.ThrowsExceptionAsync<BlobSvcException>(() =>

                _blobSvc.RevertAsync(sourcePath, destinationPath));

            Assert.IsTrue(exception.Message.Contains("Operation was canceled"));

            Assert.IsTrue(exception.InnerException.Message.Contains(cancelMessage));

        }

        [TestMethod]

        public async Task RevertAsync_ThrowsBlobSvcException_WhenWaitForCompletionFails()

        {

            // Arrange

            var sourcePath = "primary/completion-fail.txt";

            var destinationPath = "backup/completion-fail.txt";

            var errorMessage = "Wait for completion failed";

            var existingTags = new Dictionary<string, string> { { "key", "value" } };

            var mockTagResult = BlobsModelFactory.GetBlobTagResult(tags: existingTags);

            var mockTagResponse = Mock.Of<Response<GetBlobTagResult>>(r => r.Value == mockTagResult);

            _destinationBlobMock

                .Setup(b => b.GetTagsAsync(It.IsAny<BlobRequestConditions>(), It.IsAny<CancellationToken>()))

                .ReturnsAsync(mockTagResponse);

            var mockCopyOperation = new Mock<CopyFromUriOperation>();

            mockCopyOperation

                .Setup(o => o.WaitForCompletionAsync(It.IsAny<CancellationToken>()))

                .Throws(new Exception(errorMessage));

            _sourceBlobMock

                .Setup(b => b.StartCopyFromUriAsync(

                    It.IsAny<Uri>(),

                    It.IsAny<BlobCopyFromUriOptions>(),

                    It.IsAny<CancellationToken>()))

                .ReturnsAsync(mockCopyOperation.Object);

            // Act & Assert

            var exception = await Assert.ThrowsExceptionAsync<BlobSvcException>(() =>

                _blobSvc.RevertAsync(sourcePath, destinationPath));

            Assert.IsTrue(exception.Message.Contains("An unexpected error occurred"));

            Assert.IsTrue(exception.InnerException.Message.Contains(errorMessage));

        }

    }

}
