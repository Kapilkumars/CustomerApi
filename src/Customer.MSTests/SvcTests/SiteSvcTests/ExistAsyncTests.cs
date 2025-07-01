using AutoMapper;
using Customer.Metis.Common.Models.CosmosDb;
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

namespace CustomerCustomer.MSTests.SvcTests.SiteSvcTests;

[TestClass]
public class ExistAsyncTests
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
    public async Task ExistAsync_ReturnsTrue_WhenSiteExists()
    {
        // Arrange
        var siteName = "ExistingSite";
        _repositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None))
                       .ReturnsAsync(new List<SiteCosmosDb> { new SiteCosmosDb { Name = siteName } });

        // Act
        var result = await _siteSvc.ExistAsync(siteName);

        // Assert
        Assert.IsTrue(result, "ExistAsync should return true when the site exists");
        _repositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None), Times.Once, "GetAsync should be called once");
    }

    [TestMethod]
    public async Task ExistAsync_ReturnsFalse_WhenSiteDoesNotExist()
    {
        // Arrange
        var siteName = "NonExistentSite";
        _repositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None))
                       .ReturnsAsync(new List<SiteCosmosDb>());

        // Act
        var result = await _siteSvc.ExistAsync(siteName);

        // Assert
        Assert.IsFalse(result, "ExistAsync should return false when the site does not exist");
        _repositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None), Times.Once, "GetAsync should be called once");
    }

    [TestMethod]
    public async Task ExistAsync_ThrowsSiteSvcException_WhenCosmosExceptionIsThrown()
    {
        // Arrange
        var siteName = "SiteName";
        var cosmosException = new CosmosException("Cosmos error", System.Net.HttpStatusCode.BadRequest, 0, "", 0);

        _repositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None))
                       .ThrowsAsync(cosmosException);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<SiteSvcException>(() => _siteSvc.ExistAsync(siteName));

        // Verify exception details
        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, exception.HttpStatusCode, "HttpStatusCode should match the Cosmos exception");
        Assert.IsTrue(exception.Message.Contains("Cosmos Related exception"), "Exception message should indicate a Cosmos-related issue");

        _repositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None), Times.Once, "GetAsync should be called once before the exception");
        _loggerMock.Verify(logger => logger.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<CosmosException>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once, "Logger should capture the Cosmos exception");
    }

    [TestMethod]
    public async Task ExistAsync_ThrowsSiteSvcException_WhenGeneralExceptionIsThrown()
    {
        // Arrange
        var siteName = "SiteName";
        _repositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None))
                       .ThrowsAsync(new Exception("General error"));

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<SiteSvcException>(() => _siteSvc.ExistAsync(siteName));

        // Verify exception details
        Assert.IsTrue(exception.Message.Contains("Not a cosmos Related Exception"), "Exception message should indicate a general exception");
        _repositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None), Times.Once, "GetAsync should be called once before the exception");
        _loggerMock.Verify(logger => logger.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once, "Logger should capture the general exception");
    }

    [TestMethod]
    public async Task ExistAsync_With_Floor_ReturnsTrue_WhenSiteAndFloorExist()
    {
        // Arrange
        var siteName = "ExistingSite";
        var floorName = "Floor1";

        _repositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None))
                       .ReturnsAsync(new List<SiteCosmosDb>
                       {
                           new SiteCosmosDb
                           {
                               Name = siteName,
                               ChildSpaces = new List<SiteCosmosDb> { new SiteCosmosDb { Name = floorName } }
                           }
                       });

        // Act
        var result = await _siteSvc.ExistAsync(siteName, floorName);

        // Assert
        Assert.IsTrue(result, "ExistAsync should return true when both site and floor exist");
        _repositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None), Times.Once, "GetAsync should be called once");
    }

    [TestMethod]
    public async Task ExistAsync_With_Floor_ReturnsFalse_WhenSiteExistsButFloorDoesNotExist()
    {
        // Arrange
        var siteName = "ExistingSite";
        var floorName = "NonExistentFloor";

        _repositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None))
                       .ReturnsAsync(new List<SiteCosmosDb>
                       {
                           new SiteCosmosDb
                           {
                               Name = siteName,
                               ChildSpaces = new List<SiteCosmosDb> { new SiteCosmosDb { Name = "OtherFloor" } }
                           }
                       });

        // Act
        var result = await _siteSvc.ExistAsync(siteName, floorName);

        // Assert
        Assert.IsFalse(result, "ExistAsync should return false when site exists but floor does not");
        _repositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None), Times.Once, "GetAsync should be called once");
    }

    [TestMethod]
    public async Task ExistAsync_With_Floor_ReturnsFalse_WhenSiteDoesNotExist()
    {
        // Arrange
        var siteName = "NonExistentSite";
        var floorName = "Floor1";

        _repositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None))
                       .ReturnsAsync(new List<SiteCosmosDb>());

        // Act
        var result = await _siteSvc.ExistAsync(siteName, floorName);

        // Assert
        Assert.IsFalse(result, "ExistAsync should return false when site does not exist");
        _repositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None), Times.Once, "GetAsync should be called once");
    }

    [TestMethod]
    public async Task ExistAsync_With_Floor_ThrowsSiteSvcException_WhenCosmosExceptionIsThrown()
    {
        // Arrange
        var siteName = "ExistingSite";
        var floorName = "Floor1";
        var cosmosException = new CosmosException("Cosmos error", System.Net.HttpStatusCode.BadRequest, 0, "", 0);

        _repositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None))
                       .ThrowsAsync(cosmosException);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<SiteSvcException>(() => _siteSvc.ExistAsync(siteName, floorName));

        // Verify exception details
        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, exception.HttpStatusCode, "HttpStatusCode should match the Cosmos exception");
        Assert.IsTrue(exception.Message.Contains("Cosmos Related exception"), "Exception message should indicate a Cosmos-related issue");

        _repositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None), Times.Once, "GetAsync should be called once before the exception");
        _loggerMock.Verify(logger => logger.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<CosmosException>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once, "Logger should capture the Cosmos exception");
    }

    [TestMethod]
    public async Task ExistAsync_With_Floor_ThrowsSiteSvcException_WhenGeneralExceptionIsThrown()
    {
        // Arrange
        var siteName = "ExistingSite";
        var floorName = "Floor1";

        _repositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None))
                       .ThrowsAsync(new Exception("General error"));

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<SiteSvcException>(() => _siteSvc.ExistAsync(siteName, floorName));

        // Verify exception details
        Assert.IsTrue(exception.Message.Contains("Not a cosmos Related Exception"), "Exception message should indicate it's a general exception");

        _repositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None), Times.Once, "GetAsync should be called once before the exception");
        _loggerMock.Verify(logger => logger.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once, "Logger should capture the general exception");
    }
}
