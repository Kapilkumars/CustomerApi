using Azure.Storage.Blobs;
using Azure;
using CustomerCustomerApi.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;

namespace CustomerCustomer.MSTests.SvcTests.BlobSvcTests
{
    [TestClass]
    public class DeleteAsyncTests
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
            _backupContainerMock
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
        public async Task DeleteAsync_DeletesBlob_WhenBlobExists()
        {
            // Arrange
            var filePath = "backup/file-to-delete.txt";

            // Setup to return a successful deletion response
            _blobClientMock
                .Setup(b => b.DeleteIfExistsAsync(
                    DeleteSnapshotsOption.None,
                    It.IsAny<BlobRequestConditions>(),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(Response.FromValue(true, Mock.Of<Response>()));

            // Act
            await _blobSvc.DeleteAsync(filePath);

            // Assert
            _backupContainerMock.Verify(c => c.GetBlobClient(filePath), Times.Once);
            _blobClientMock.Verify(b => b.DeleteIfExistsAsync(
                DeleteSnapshotsOption.None,
                It.IsAny<BlobRequestConditions>(),
                It.IsAny<CancellationToken>()), Times.Once);
        }
        [TestMethod]
        public async Task DeleteAsync_HandlesNonExistentBlob_WhenBlobDoesNotExist()
        {
            // Arrange
            var filePath = "backup/non-existing-file.txt";

            _blobClientMock
                .Setup(b => b.DeleteIfExistsAsync(
                    DeleteSnapshotsOption.None,
                    It.IsAny<BlobRequestConditions>(),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(Response.FromValue(false, Mock.Of<Response>()));

            // Act
            await _blobSvc.DeleteAsync(filePath);

            // Assert
            _backupContainerMock.Verify(c => c.GetBlobClient(filePath), Times.Once);
            _blobClientMock.Verify(b => b.DeleteIfExistsAsync(
                DeleteSnapshotsOption.None,
                It.IsAny<BlobRequestConditions>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [TestMethod]
        public async Task DeleteAsync_PropagatesExceptions_WhenDeleteThrows()
        {
            // Arrange
            var filePath = "backup/error-file.txt";
            var errorMessage = "Delete operation failed";

            _blobClientMock
                .Setup(b => b.DeleteIfExistsAsync(
                    DeleteSnapshotsOption.None,
                    It.IsAny<BlobRequestConditions>(),
                    It.IsAny<CancellationToken>()
                ))
                .ThrowsAsync(new RequestFailedException(errorMessage));

            // Act & Assert
            var exception = await Assert.ThrowsExceptionAsync<RequestFailedException>(() =>
                _blobSvc.DeleteAsync(filePath));

            Assert.AreEqual(errorMessage, exception.Message);
            _backupContainerMock.Verify(c => c.GetBlobClient(filePath), Times.Once);
            _blobClientMock.Verify(b => b.DeleteIfExistsAsync(
                DeleteSnapshotsOption.None,
                It.IsAny<BlobRequestConditions>(),
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

    }
}
