using AutoMapper;
using Customer.Metis.Common.Space;
using Customer.Metis.Logging.Correlation;
using CustomerCustomerApi.Exceptions;
using CustomerCustomerApi.Interfaces;
using CustomerCustomerApi.Models;
using CustomerCustomerApi.Models.Site;
using CustomerCustomerApi.Services;
using Microsoft.Azure.CosmosRepository;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;


namespace CustomerCustomer.MSTests.SvcTests.SiteSvcTests
{

    [TestClass]
    public class UpdateSingleSiteAsyncTests
    {

        private Mock<ILogger<SiteSvc>> _loggerMock;
        private Mock<IRepository<SiteCosmosDb>> _repositoryMock;
        private Mock<IRepositoryFactory> _repositoryFactoryMock;
        private Mock<IMapper> _mapperMock;
        private Mock<ICorrelationIdGenerator> _correlationIdGeneratorMock;
        private Mock<IBlobSvc> _blobSvc;
        private SiteSvc _siteSvc;

        [TestInitialize]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<SiteSvc>>();
            _repositoryMock = new Mock<IRepository<SiteCosmosDb>>();
            _repositoryFactoryMock = new Mock<IRepositoryFactory>();
            _mapperMock = new Mock<IMapper>();
            _correlationIdGeneratorMock = new Mock<ICorrelationIdGenerator>();
            _blobSvc = new Mock<IBlobSvc>();

            _correlationIdGeneratorMock.Setup(x => x.Get()).Returns("correlation-id");
            _repositoryFactoryMock.Setup(x => x.RepositoryOf<SiteCosmosDb>()).Returns(_repositoryMock.Object);

            _siteSvc = new SiteSvc(
                _loggerMock.Object,
                _repositoryFactoryMock.Object,
                _mapperMock.Object,
                _correlationIdGeneratorMock.Object,
                _blobSvc.Object);
        }

        //[TestMethod]
        //public async Task UpdateGeoJsonSingleAsync_ShouldUpdateSite_WhenGeoJsonExistsAndValid()
        //{
        //    // Arrange
        //    var model = new GeoJsonUpdateModel
        //    {
        //        SiteId = "site123",
        //        SpaceId = "space456",
        //        Geometry = new GeometryBase(),
        //        GeoJsonFile = new FileModel(),
        //        ChildSpaces = JsonConvert.SerializeObject(new SiteModel
        //        {
        //            ChildSpaces = new List<SiteModel> { new SiteModel { Id = "child1" } }
        //        })
        //    };

        //    var siteItem = new SiteCosmosDb
        //    {
        //        Id = "site123",
        //        Geometry = null,
        //        ChildSpaces = new List<SiteCosmosDb> { new SiteCosmosDb { Id = "space456" } }
        //    };

        //    var updatedChild = new SiteCosmosDb { Id = "space456" };

        //    _blobSvc.Setup(x => x.ExistsAsync(It.IsAny<string>())).ReturnsAsync(true);
        //    _blobSvc.Setup(x => x.ExistsBackupAsync(It.IsAny<string>())).ReturnsAsync(true);
        //    _blobSvc.Setup(x => x.MoveAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
        //    .Returns(Task.CompletedTask);

        //    _repositoryMock.Setup(r => r.GetAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(siteItem);
        //    _repositoryMock.Setup(r => r.UpdateAsync(It.IsAny<SiteCosmosDb>(), false, It.IsAny<CancellationToken>())).ReturnsAsync(siteItem);

        //    _mapperMock.Setup(m => m.Map<GeoJsonUpdateModel>(It.IsAny<SiteModel>())).Returns(model);

        //    // Act
        //    var result = await _siteSvc.UpdateFileAsync(model);

        //    // Assert
        //    Assert.IsNotNull(result);
        //    Assert.AreEqual(model.SiteId, result.SiteId);
        //    Assert.AreEqual(model.SpaceId, result.SpaceId);

        //    _blobSvc.Verify(x => x.DeleteAsync(It.IsAny<string>()), Times.Once);
        //    _blobSvc.Verify(x => x.MoveAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        //    _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<SiteCosmosDb>(), false, It.IsAny<CancellationToken>()), Times.Once);

        //}


        //[TestMethod]
        //public async Task UpdateGeoJsonSingleAsync_ShouldLogError_WhenMoveThrowsException()
        //{
        //    // Arrange
        //    var model = new GeoJsonUpdateModel
        //    {
        //        SiteId = "site123",
        //        SpaceId = "space456",
        //        Geometry = new GeometryBase(),
        //        GeoJsonFile = new FileModel(),
        //        ChildSpaces = JsonConvert.SerializeObject(new SiteModel())
        //    };

        //    _blobSvc.Setup(x => x.ExistsAsync(It.IsAny<string>())).ReturnsAsync(true);
        //    _blobSvc.Setup(x => x.ExistsBackupAsync(It.IsAny<string>())).ReturnsAsync(true);
        //    _blobSvc.Setup(x => x.DeleteAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        //    _blobSvc.Setup(x => x.MoveAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
        //            .Throws(new Exception("Move failed"));

        //    _repositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(new SiteCosmosDb
        //    {
        //        Id = "site123",
        //        ChildSpaces = new List<SiteCosmosDb>()
        //    });
        //    _repositoryMock.Setup(x => x.UpdateAsync(It.IsAny<SiteCosmosDb>(), false, It.IsAny<CancellationToken>())).ReturnsAsync(new SiteCosmosDb());

        //    _mapperMock.Setup(m => m.Map<GeoJsonUpdateModel>(It.IsAny<SiteModel>())).Returns(model);

        //    // Act
        //    var result = await _siteSvc.UpdateFileAsync(model);

        //    // Assert
        //    Assert.IsNotNull(result);

        //    _loggerMock.Verify(
        //        l => l.Log(
        //            LogLevel.Error,
        //            It.IsAny<EventId>(),
        //            It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("CorrelationId: correlation-id")),
        //            It.IsAny<Exception>(),
        //            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        //        ),
        //        Times.Once
        //      );


        //}

        //[TestMethod]
        //public async Task UpdateGeoJsonSingleAsync_ShouldThrowSiteSvcException_WhenUnexpectedExceptionOccurs()
        //{
        //    var model = new GeoJsonUpdateModel { SiteId = "site123", SpaceId = "space456", ChildSpaces = "invalid json" };

        //    await Assert.ThrowsExceptionAsync<SiteSvcException>(() => _siteSvc.UpdateFileAsync(model));

        //    _loggerMock.Verify(
        //         l => l.Log(
        //             LogLevel.Error,
        //             It.IsAny<EventId>(),
        //             It.Is<It.IsAnyType>((v, t) =>
        //                 v.ToString().Contains("GeoJSON single update failed") &&
        //                 v.ToString().Contains("CorrelationId: correlation-id")),
        //             It.IsAny<Exception>(),
        //             It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        //         Times.Once);

        //}


    }
}