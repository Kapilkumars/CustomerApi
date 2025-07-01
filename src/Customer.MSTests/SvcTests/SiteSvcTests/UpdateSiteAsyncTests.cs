using AutoMapper;
using Customer.Metis.Common.Models.Responses;
using Customer.Metis.Logging.Correlation;
using CustomerCustomerApi.Exceptions;
using CustomerCustomerApi.Interfaces;
using CustomerCustomerApi.Models;
using CustomerCustomerApi.Models.Site;
using CustomerCustomerApi.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.CosmosRepository;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;

namespace CustomerCustomer.MSTests.SvcTests.SiteSvcTests;

[TestClass]
public class UpdateSiteAsyncTests
{
    private Mock<ILogger<SiteSvc>> _loggerMock;
    private Mock<IRepository<SiteCosmosDb>> _repositoryMock;
    private Mock<IRepositoryFactory> _repositoryFactoryMock;
    private Mock<IMapper> _mapperMock;
    private Mock<ICorrelationIdGenerator> _correlationIdGeneratorMock;
    private SiteSvc _siteSvc;
    private Mock<IBlobSvc> _blobSvc;
    

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<SiteSvc>>();
        _repositoryMock = new Mock<IRepository<SiteCosmosDb>>();
        _repositoryFactoryMock = new Mock<IRepositoryFactory>();
        _mapperMock = new Mock<IMapper>();
        _correlationIdGeneratorMock = new Mock<ICorrelationIdGenerator>();
        _blobSvc = new Mock<IBlobSvc>();

        
        _repositoryFactoryMock
            .Setup(x => x.RepositoryOf<SiteCosmosDb>())
            .Returns(_repositoryMock.Object);

        _siteSvc = new SiteSvc(
            _loggerMock.Object,
            _repositoryFactoryMock.Object,
            _mapperMock.Object,
            _correlationIdGeneratorMock.Object, _blobSvc.Object);
    }

    [TestMethod]
    public async Task UpdateSiteAsync_UpdatesSite_WhenValidModelAndIdProvided()
    {
        // Arrange
        var siteId = "site123";
        var siteModel = new SiteModel
        {
            Name = "NewSite",
            ActiveScene = default,
            Schedule = "test",
            Geometry = default,
            ChildSpaces = new List<SiteModel> { new SiteModel { Id = "1", Name = "ChildSpace1" } }
        };

        var existingSite = new SiteCosmosDb 
        {
            Id = siteId,
            Name = siteModel.Name
        };

        _repositoryMock.Setup(repo => repo.GetAsync(siteId, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSite);

        _repositoryMock.Setup(repo =>  repo.UpdateAsync(It.IsAny<SiteCosmosDb>(), It.IsAny<bool>(), CancellationToken.None))
            .ReturnsAsync(existingSite);

        _mapperMock.Setup(mapper => mapper.Map<List<SiteCosmosDb>>(siteModel.ChildSpaces))
                   .Returns(new List<SiteCosmosDb> { new SiteCosmosDb { Name = "ChildSpace1" } });

        _mapperMock.Setup(mapper => mapper.Map<SiteResponseModel>(existingSite))
                   .Returns(new SiteResponseModel { Schedule = "test" });

        var result = await _siteSvc.UpdateSiteAsync(siteModel, siteId);

        Assert.IsNotNull(result);
        Assert.AreEqual("test", result.Schedule); // test mapped property

        _repositoryMock.Verify(repo => repo.GetAsync(siteId, null, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<SiteCosmosDb>(), It.IsAny<bool>(), CancellationToken.None), Times.Once);
    }

    [TestMethod]
    public async Task UpdateSiteAsync_ThrowsSiteSvcException_WhenSiteNameIsNotUniqueInChildSpaces()
    {
        // Arrange
        var siteId = "site123";
        var siteModel = new SiteModel
        {
            Name = "ParentSite",
            ChildSpaces = new List<SiteModel>
        {
            new SiteModel { Name = "DuplicateName" },
            new SiteModel { Name = "DuplicateName" }
        }
        };

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<SiteSvcException>(() => _siteSvc.UpdateSiteAsync(siteModel, siteId));

        // Verify
        Assert.AreEqual(HttpStatusCode.Conflict, exception.HttpStatusCode);
        Assert.IsTrue(exception.Message.Contains("Site name must be unique among child spaces"));

        // Verify no repository calls were made since validation fails first
        _repositoryMock.Verify(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<SiteCosmosDb>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task UpdateSiteAsync_ThrowsSiteSvcException_WhenCosmosExceptionIsThrown()
    {
        var siteId = "site123";
        var siteModel = new SiteModel
        {
            Name = "NewSite",
            ChildSpaces = new List<SiteModel> { new SiteModel { Name = "ChildSpace1" } }
        };

        var cosmosException = new CosmosException("Cosmos error", HttpStatusCode.BadRequest, 0, "", 0);

        _repositoryMock.Setup(repo => repo.GetAsync(siteId, null, It.IsAny<CancellationToken>()))
               .ThrowsAsync(cosmosException);

        _correlationIdGeneratorMock.Setup(x => x.Get()).Returns("correlation-id");

        var exception = await Assert.ThrowsExceptionAsync<SiteSvcException>(() => _siteSvc.UpdateSiteAsync(siteModel, siteId));

        Assert.AreEqual(HttpStatusCode.BadRequest, exception.HttpStatusCode);
        Assert.IsTrue(exception.Message.Contains("Cosmos DB error"));

        _repositoryMock.Verify(repo => repo.GetAsync(siteId, null, It.IsAny<CancellationToken>()), Times.Once);
        _loggerMock.Verify(logger => logger.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<CosmosException>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once, "Logger should capture the Cosmos exception");
    }

    [TestMethod]
    public async Task UpdateSiteAsync_ThrowsSiteSvcException_WhenGeneralExceptionIsThrown()
    {
        var siteId = "site123";
        var siteModel = new SiteModel
        {
            Name = "NewSite",
            ChildSpaces = new List<SiteModel> { new SiteModel { Name = "ChildSpace1" } }
        };

        _repositoryMock.Setup(repo => repo.GetAsync(siteId, null, It.IsAny<CancellationToken>()))
               .ThrowsAsync(new Exception("General error"));

        _correlationIdGeneratorMock.Setup(x => x.Get()).Returns("correlation-id");

        var exception = await Assert.ThrowsExceptionAsync<SiteSvcException>(() => _siteSvc.UpdateSiteAsync(siteModel, siteId));

        Assert.IsTrue(exception.Message.Contains($"Update failed for site {siteId}"));

        _repositoryMock.Verify(repo => repo.GetAsync(siteId, null, It.IsAny<CancellationToken>()), Times.Once);
    }
}