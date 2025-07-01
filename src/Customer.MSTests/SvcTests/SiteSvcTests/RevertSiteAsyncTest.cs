using CustomerCustomerApi.Exceptions;
using CustomerCustomerApi.Interfaces;
using CustomerCustomerApi.Services;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using Customer.Metis.Logging.Correlation;
using AutoMapper;
using Microsoft.Azure.CosmosRepository;

namespace CustomerCustomer.MSTests.SvcTests.SiteSvcTests
{
    [TestClass]
    public class RevertSiteAsyncTests
    {
        private Mock<ILogger<SiteSvc>> _loggerMock;
        private Mock<IBlobSvc> _blobSvcMock;
        private Mock<ICorrelationIdGenerator> _correlationIdGeneratorMock;
        private SiteSvc _siteSvc;

        [TestInitialize]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<SiteSvc>>();
            _blobSvcMock = new Mock<IBlobSvc>();
            _correlationIdGeneratorMock = new Mock<ICorrelationIdGenerator>();
            var repositoryFactoryMock = new Mock<IRepositoryFactory>();
            var mapperMock = new Mock<IMapper>();
            _siteSvc = new SiteSvc(
                _loggerMock.Object,
                repositoryFactoryMock.Object,
                mapperMock.Object,
                _correlationIdGeneratorMock.Object,
                _blobSvcMock.Object);
        }


        [TestMethod]
        public async Task RevertSiteAsync_ReturnsSuccess_WhenBackupExists()
        {
            // Arrange
            string siteId = "site123";
            string floorId = "floor456";
            string path = $"{siteId}/{floorId}.json";

            _blobSvcMock
                .Setup(svc => svc.ExistsBackupAsync(path))
                .ReturnsAsync(true);  // Simulate backup exists

            _blobSvcMock
                .Setup(svc => svc.RevertAsync(path, path, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);  // Simulate revert

            // Act
            var result = await _siteSvc.RevertSiteAsync(siteId, floorId);

            // Assert
            Assert.AreEqual("Revert successful", result);  // Check success message

            // Verify that ExistsBackupAsync was called once, and RevertAsync was never called
            _blobSvcMock.Verify(svc => svc.ExistsBackupAsync(path), Times.Once);
            _blobSvcMock.Verify(svc => svc.RevertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }


        [TestMethod]
        public async Task RevertSiteAsync_ReturnsNull_WhenBackupDoesNotExist()
        {
            // Arrange
            string siteId = "site123";
            string floorId = "floor456";
            string path = $"{siteId}/{floorId}.json";

            _blobSvcMock
                .Setup(svc => svc.ExistsBackupAsync(path))
                .ReturnsAsync(false);

            // Act
            var result = await _siteSvc.RevertSiteAsync(siteId, floorId);

            // Assert
            Assert.IsNull(result);
            _blobSvcMock.Verify(svc => svc.ExistsBackupAsync(path), Times.Once);
            _blobSvcMock.Verify(svc => svc.RevertAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);

        }

        [TestMethod]
        public async Task RevertSiteAsync_ThrowsSiteSvcException_WhenExceptionOccurs()
        {
            // Arrange
            string siteId = "site123";
            string floorId = "floor456";
            string path = $"{siteId}/{floorId}.json";

            _blobSvcMock
                .Setup(svc => svc.ExistsBackupAsync(path))
                .ThrowsAsync(new Exception("Simulated failure"));

            _correlationIdGeneratorMock
                .Setup(x => x.Get())
                .Returns("correlation-id-xyz");

            // Act & Assert
            var ex = await Assert.ThrowsExceptionAsync<SiteSvcException>(
                () => _siteSvc.RevertSiteAsync(siteId, floorId));

            Assert.IsTrue(ex.Message.Contains("Unexpected error during revert"));
            Assert.IsTrue(ex.Message.Contains("correlation-id-xyz"));

            _blobSvcMock.Verify(svc => svc.ExistsBackupAsync(path), Times.Once);
        }
    }
}