using AutoMapper;
using Customer.Metis.Common.Models.CosmosDb;
using CustomerCustomerApi.Exceptions;
using CustomerCustomerApi.Services;
using Customer.Metis.Logging.Correlation;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.CosmosRepository;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using System.Net;
using CustomerCustomerApi.Interfaces;
using CustomerCustomerApi.Models;

namespace CustomerCustomer.MSTests.SvcTests.SiteSvcTests;

[TestClass]
public class DeleteSiteAsyncTests
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
            _correlationIdGeneratorMock.Object,
            _blobSvc.Object);
    }

    [TestMethod]
    public async Task DeleteSiteAsync_DeletesSite_WhenSiteExists()
    {
        // Arrange
        var siteId = "site123";
        var siteList = new List<SiteCosmosDb>
        {
            new SiteCosmosDb { Id = siteId, Name = "TestSite" }
        };

        _repositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None))
                       .ReturnsAsync(siteList);

        // Act
        await _siteSvc.DeleteSiteAsync(siteId);

        // Assert
        _repositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None), Times.Once, "GetAsync should be called once");
        _repositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<SiteCosmosDb>(), CancellationToken.None), Times.Once, "DeleteAsync should be called once");
    }

    [TestMethod]
    public async Task DeleteSiteAsync_ThrowsSiteSvcException_WhenSiteNotFound()
    {
        // Arrange
        var siteId = "site123";
        var emptySiteList = new List<SiteCosmosDb>();

        _repositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None))
                       .ReturnsAsync(emptySiteList);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<SiteSvcException>(() => _siteSvc.DeleteSiteAsync(siteId));

        Assert.AreEqual(HttpStatusCode.Conflict, exception.HttpStatusCode, "HttpStatusCode should be Conflict when no site is found");
        Assert.IsTrue(exception.InnerException!.Message.Contains("Site name must be unique"), "Exception message should indicate the issue with site not being found");

        _repositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None), Times.Once, "GetAsync should be called once");
        _repositoryMock.Verify(repo => repo.DeleteAsync(It.IsAny<SiteCosmosDb>(), CancellationToken.None), Times.Never, "DeleteAsync should not be called when site is not found");
    }

    [TestMethod]
    public async Task DeleteSiteAsync_ThrowsSiteSvcException_WhenCosmosExceptionIsThrown()
    {
        // Arrange
        var siteId = "site123";
        var cosmosException = new CosmosException("Cosmos error", System.Net.HttpStatusCode.BadRequest, 0, "", 0);

        _repositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None))
                       .ThrowsAsync(cosmosException);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<SiteSvcException>(() => _siteSvc.DeleteSiteAsync(siteId));

        Assert.AreEqual(HttpStatusCode.BadRequest, exception.HttpStatusCode, "HttpStatusCode should match the CosmosException's status code");
        Assert.IsTrue(exception.Message.Contains("Cosmos Related exception"), "Exception message should indicate a Cosmos-related issue");

        _repositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None), Times.Once, "GetAsync should be called once");
        _loggerMock.Verify(logger => logger.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<CosmosException>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once, "Logger should capture the Cosmos exception");
    }

    [TestMethod]
    public async Task DeleteSiteAsync_ThrowsSiteSvcException_WhenGeneralExceptionIsThrown()
    {
        // Arrange
        var siteId = "site123";
        _repositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None))
                       .ThrowsAsync(new Exception("General error"));

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<SiteSvcException>(() => _siteSvc.DeleteSiteAsync(siteId));

        Assert.IsTrue(exception.Message.Contains("Not a cosmos Related Exception"), "Exception message should indicate it's a general exception");
        _repositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None), Times.Once, "GetAsync should be called once before the exception");
        _loggerMock.Verify(logger => logger.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once, "Logger should capture the general exception");
    }
}
