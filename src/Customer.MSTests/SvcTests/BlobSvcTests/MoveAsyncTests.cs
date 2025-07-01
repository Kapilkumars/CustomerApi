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

    public class MoveAsyncTests

    {

        private Mock<BlobClient> _sourceBlobMock;

        private Mock<BlobClient> _destinationBlobMock;

        private Mock<BlobContainerClient> _primaryContainerMock;

        private Mock<BlobContainerClient> _backupContainerMock;

        private Mock<ILogger<BlobSvc>> _loggerMock;

        private BlobSvc _blobSvc;

        private Uri _sourceUri;

        private CopyFromUriOperation _copyOperation;

        [TestInitialize]

        public void Setup()

        {

            _sourceBlobMock = new Mock<BlobClient>();

            _destinationBlobMock = new Mock<BlobClient>();

            _primaryContainerMock = new Mock<BlobContainerClient>();

            _backupContainerMock = new Mock<BlobContainerClient>();

            _loggerMock = new Mock<ILogger<BlobSvc>>();

            _sourceUri = new Uri("https://test.blob.core.windows.net/container/source-blob");

            // Create a real operation that will complete immediately

            var mockOperation = new Mock<CopyFromUriOperation>(MockBehavior.Strict);

            _destinationBlobMock

                .Setup(b => b.StartCopyFromUriAsync(

                    It.IsAny<Uri>(),

                    It.IsAny<BlobCopyFromUriOptions>(),

                    It.IsAny<CancellationToken>()))

                .ReturnsAsync(mockOperation.Object);


            var options = new BlobSvcOptions(

                "primary",

                "UseDevelopmentStorage=true",

                "backup"

            );

            // Setup source blob mock

            _sourceBlobMock.Setup(b => b.Uri).Returns(_sourceUri);

            // Setup container mocks to return the appropriate blob clients

            _primaryContainerMock

                .Setup(c => c.GetBlobClient(It.IsAny<string>()))

                .Returns(_sourceBlobMock.Object);

            _backupContainerMock

                .Setup(c => c.GetBlobClient(It.IsAny<string>()))

                .Returns(_destinationBlobMock.Object);

            // Setup destination blob to return copy operation

            _destinationBlobMock

                .Setup(b => b.StartCopyFromUriAsync(

                    It.IsAny<Uri>(),

                    It.IsAny<BlobCopyFromUriOptions>(),

                    It.IsAny<CancellationToken>()))

                .ReturnsAsync(mockOperation.Object);


            // Create service with mocked dependencies

            _blobSvc = new BlobSvc(

                options,

                _loggerMock.Object,

                _primaryContainerMock.Object,

                _backupContainerMock.Object

            );

        }

        [TestMethod]

        public async Task MoveAsync_CopiesFileWithTags_WhenSuccessful()

        {

            // Arrange

            var sourcePath = "source/file.txt";

            var destinationPath = "destination/file.txt";

            var spaceId = "space123";

            var cancellationToken = new CancellationToken();

            var existingTags = new Dictionary<string, string> { { "spaceId", spaceId } };

            var mockTagResult = BlobsModelFactory.GetBlobTagResult(tags: existingTags);

            var mockTagResponse = Mock.Of<Response<GetBlobTagResult>>(r => r.Value == mockTagResult);

            _sourceBlobMock

                .Setup(b => b.GetTagsAsync(It.IsAny<BlobRequestConditions>(), cancellationToken))

                .ReturnsAsync(mockTagResponse)

                .Verifiable();

            var mockBlobProperties = Mock.Of<BlobProperties>();

            var mockCopyOperation = new Mock<CopyFromUriOperation>();

            var mockCompletionResponse = Mock.Of<Response<long>>(r => r.Value == 1L);

            mockCopyOperation

                .Setup(o => o.WaitForCompletionAsync(It.IsAny<CancellationToken>()))

                .Returns(new ValueTask<Response<long>>(mockCompletionResponse));


            _destinationBlobMock

                .Setup(b => b.StartCopyFromUriAsync(It.IsAny<Uri>(), It.IsAny<BlobCopyFromUriOptions>(), cancellationToken))

                .ReturnsAsync(mockCopyOperation.Object)

                .Verifiable();

            // Act

            await _blobSvc.MoveAsync(sourcePath, destinationPath, cancellationToken);

            // Assert

            _sourceBlobMock.Verify();

            _destinationBlobMock.Verify();

            mockCopyOperation.Verify(o => o.WaitForCompletionAsync(cancellationToken), Times.Once);

        }


        [TestMethod]

        public async Task MoveAsync_UsesEmptyTags_WhenGetTagsReturnsNull()

        {

            // Arrange

            var sourcePath = "source/file.txt";

            var destinationPath = "destination/file.txt";

            var spaceId = "space123";

            var cancellationToken = new CancellationToken();

            Response<GetBlobTagResult> nullTagResponse = null!;

            _sourceBlobMock

                .Setup(b => b.GetTagsAsync(It.IsAny<BlobRequestConditions>(), cancellationToken))

                .ReturnsAsync(nullTagResponse)

                .Verifiable();

            var mockBlobProperties = Mock.Of<BlobProperties>();

            var mockCopyOperation = new Mock<CopyFromUriOperation>();

            var mockCompletionResponse = Mock.Of<Response<long>>(r => r.Value == 1L);

            mockCopyOperation

                .Setup(o => o.WaitForCompletionAsync(It.IsAny<CancellationToken>()))

                .Returns(new ValueTask<Response<long>>(mockCompletionResponse));


            _destinationBlobMock

                .Setup(b => b.StartCopyFromUriAsync(It.IsAny<Uri>(), It.IsAny<BlobCopyFromUriOptions>(), cancellationToken))

                .ReturnsAsync(mockCopyOperation.Object)

                .Verifiable();

            // Act

            await _blobSvc.MoveAsync(sourcePath, destinationPath, cancellationToken);

            // Assert

            _sourceBlobMock.Verify();

            _destinationBlobMock.Verify();

            mockCopyOperation.Verify(o => o.WaitForCompletionAsync(cancellationToken), Times.Once);

        }


        [TestMethod]

        public async Task MoveAsync_PassesCancellationToken_ToAllOperations()

        {

            // Arrange

            var sourcePath = "source/file.txt";

            var destinationPath = "destination/file.txt";

            var spaceId = "space789";

            var cancellationToken = new CancellationToken();

            var existingTags = new Dictionary<string, string> { { "spaceId", spaceId } };

            var mockTagResult = BlobsModelFactory.GetBlobTagResult(tags: existingTags);

            var mockResponse = Mock.Of<Response<GetBlobTagResult>>(r => r.Value == mockTagResult);

            var mockCopyOperation = new Mock<CopyFromUriOperation>();

            _sourceBlobMock

                .Setup(b => b.GetTagsAsync(It.IsAny<BlobRequestConditions>(), cancellationToken))

                .ReturnsAsync(mockResponse)

                .Verifiable();

            _destinationBlobMock

                .Setup(b => b.StartCopyFromUriAsync(It.IsAny<Uri>(), It.IsAny<BlobCopyFromUriOptions>(), cancellationToken))

                .ReturnsAsync(mockCopyOperation.Object)

                .Verifiable();

            // Act

            await _blobSvc.MoveAsync(sourcePath, destinationPath, cancellationToken);

            // Assert

            _sourceBlobMock.Verify();

            _destinationBlobMock.Verify();

        }




        [TestMethod]

        public async Task MoveAsync_ThrowsBlobSvcException_WhenCopyFails()

        {

            // Arrange

            var sourcePath = "source/error-file.txt";

            var destinationPath = "destination/error-file.txt";

            var spaceId = "errorSpace";

            var errorMessage = "Copy operation failed";

            _destinationBlobMock

                .Setup(b => b.StartCopyFromUriAsync(

                    It.IsAny<Uri>(),

                    It.IsAny<BlobCopyFromUriOptions>(),

                    It.IsAny<CancellationToken>()))

                .Throws(new Exception(errorMessage));

            // Act & Assert

            var exception = await Assert.ThrowsExceptionAsync<BlobSvcException>(() =>

                _blobSvc.MoveAsync(sourcePath, destinationPath));

            Assert.IsTrue(exception.Message.Contains("An unexpected error occurred"));

            Assert.IsTrue(exception.InnerException.Message.Contains(errorMessage));

        }

        [TestMethod]

        public async Task MoveAsync_ThrowsBlobSvcException_WhenOperationCanceled()

        {

            // Arrange

            var sourcePath = "source/canceled-file.txt";

            var destinationPath = "destination/canceled-file.txt";

            var spaceId = "canceledSpace";

            var cancelMessage = "Operation was canceled by user";

            // Setup copy operation to throw OperationCanceledException

            _destinationBlobMock

                .Setup(b => b.StartCopyFromUriAsync(

                    It.IsAny<Uri>(),

                    It.IsAny<BlobCopyFromUriOptions>(),

                    It.IsAny<CancellationToken>()))

                .Throws(new OperationCanceledException(cancelMessage));

            // Act & Assert

            var exception = await Assert.ThrowsExceptionAsync<BlobSvcException>(() =>

                _blobSvc.MoveAsync(sourcePath, destinationPath));

            Assert.IsTrue(exception.Message.Contains("Operation was canceled"));

            Assert.IsTrue(exception.InnerException.Message.Contains(cancelMessage));

        }

    }

}
