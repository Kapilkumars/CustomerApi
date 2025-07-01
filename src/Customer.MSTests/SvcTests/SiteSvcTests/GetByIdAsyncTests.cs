using AutoMapper;
using Customer.Metis.Common.Models.CosmosDb;
using Customer.Metis.Common.Models.Responses;
using Customer.Metis.Logging.Correlation;
using CustomerCustomerApi.Exceptions;
using CustomerCustomerApi.Interfaces;
using CustomerCustomerApi.Models;
using CustomerCustomerApi.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.CosmosRepository;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using System.Net;

namespace CustomerCustomer.MSTests.SvcTests.SiteSvcTests;

[TestClass]
public class GetByIdAsyncTests
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
    public async Task GetByIdAsync_ThrowsSiteSvcException_WhenSiteNotFound()
    {
        // Arrange
        var siteId = "site123";
        var emptySiteList = new List<SiteCosmosDb>();
        _repositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None))
                    .ReturnsAsync(emptySiteList);
        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<SiteSvcException>(async () => await _siteSvc.GetSiteByIdAsync(siteId));

        Assert.AreEqual(HttpStatusCode.Conflict, exception.HttpStatusCode, "HttpStatusCode should be Conflict when no site is found");

        _repositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None), Times.Once, "GetAsync should be called once");
        _mapperMock.Verify(mapper => mapper.Map<SiteResponseModel>(It.IsAny<SiteCosmosDb>()), Times.Never, "Mapper should not be called when site is not found");
    }


    [TestMethod]
    public async Task GetByIdAsync_ThrowsSiteSvcException_WhenCosmosExceptionIsThrown()
    {
        var siteId = "site123";
        var cosmosException = new CosmosException("Cosmos error", HttpStatusCode.BadRequest, 0, "", 0);

        _repositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None))
                       .ThrowsAsync(cosmosException);

        var exception = await Assert.ThrowsExceptionAsync<SiteSvcException>(() => _siteSvc.GetSiteByIdAsync(siteId));

        Assert.AreEqual(HttpStatusCode.BadRequest, exception.HttpStatusCode);
        Assert.IsTrue(exception.Message.Contains("Cosmos Related exception"));

        _repositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None), Times.Once);
        _loggerMock.Verify(logger => logger.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => true),
            It.IsAny<CosmosException>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once, "Logger should capture the Cosmos exception");
    }

    [TestMethod]
    public async Task GetByIdAsync_ThrowsSiteSvcException_WhenGeneralExceptionIsThrown()
    {
        // Arrange
        var siteId = "site123";
        _repositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None))
                       .ThrowsAsync(new Exception("General error"));

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<SiteSvcException>(() => _siteSvc.GetSiteByIdAsync(siteId));

        Assert.IsTrue(exception.Message.Contains("Not a cosmos Related Exception"), "Exception message should indicate it's a general exception");

        _repositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None), Times.Once);
        _loggerMock.Verify(logger => logger.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => true),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once, "Logger should capture the general exception");
    }

    [TestMethod]
    public async Task GetByIdAsync_ReturnsSite_WhenSiteExists()
    {
        // Arrange
        var siteId = "site123";
        var siteList = new List<SiteCosmosDb>
        {
            new SiteCosmosDb { Id = siteId, Name = "TestSite" }
        };

        _repositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None))
                       .ReturnsAsync(siteList);

        var siteResponse = new SiteResponseModel { Id = siteId, Name = "TestSite" };
        _mapperMock.Setup(mapper => mapper.Map<SiteResponseModel>(siteList.First()))
                   .Returns(siteResponse);

        // Act
        var result = await _siteSvc.GetSiteByIdAsync(siteId);

        // Assert
        Assert.IsNotNull(result, "Result should not be null");
        Assert.AreEqual(siteId, result.Id, "Result should have the correct site Id");
        _repositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None), Times.Once, "GetAsync should be called once");
        _mapperMock.Verify(mapper => mapper.Map<SiteResponseModel>(siteList.First()), Times.Once, "Mapper should map the site correctly");
    }
}