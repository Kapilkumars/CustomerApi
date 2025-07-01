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
using System.Linq.Expressions;
using System.Net;

namespace CustomerCustomer.MSTests.SvcTests.SiteSvcTests;

[TestClass]
public class CreateSiteAsyncTests
{
    private Mock<ILogger<SiteSvc>> _loggerMock;
    private Mock<IRepository<SiteCosmosDb>> _repositoryMock;
    private Mock<IRepositoryFactory> _repositoryFactoryMock;
    private Mock<IMapper> _mapperMock;
    private Mock<ICorrelationIdGenerator> _correlationIdGeneratorMock;
    private Mock<IBlobSvc> _blobSvc;
    private SiteSvc _spaceSvc;

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

        _spaceSvc = new SiteSvc(
            _loggerMock.Object,
            _repositoryFactoryMock.Object,
            _mapperMock.Object,
            _correlationIdGeneratorMock.Object,
            _blobSvc.Object);
    }

    [TestMethod]
    public async Task CreateSiteAsync_ReturnsSiteResponseModel_WhenSiteIsSuccessfullyCreated()
    {
        // Arrange
        var siteModel = new SiteModel { Name = "NewSite", ChildSpaces = new List<SiteModel>() };
        var cosmosItem = new SiteCosmosDb { Id = "123", Name = "NewSite" };
        var createdItem = new SiteResponseModel { Id = "123", Name = "NewSite" };

        _repositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<SiteCosmosDb>(), CancellationToken.None))
                       .ReturnsAsync(cosmosItem);

        _mapperMock.Setup(mapper => mapper.Map<SiteCosmosDb>(siteModel))
                   .Returns(cosmosItem);

        _mapperMock.Setup(mapper => mapper.Map<SiteResponseModel>(cosmosItem))
                   .Returns(createdItem);

        _repositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None))
                       .ReturnsAsync(new List<SiteCosmosDb>());

        // Act
        var result = await _spaceSvc.CreateSiteAsync(siteModel);

        // Assert
        Assert.IsNotNull(result, "Result should not be null");
        Assert.AreEqual("123", result.Id, "SiteId should match the created site's id");
        Assert.AreEqual("NewSite", result.Name, "SiteName should match the created site's name");

        _repositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<SiteCosmosDb>(), CancellationToken.None), Times.Once, "CreateAsync should be called once");
        _mapperMock.Verify(mapper => mapper.Map<SiteCosmosDb>(siteModel), Times.Once, "Map to CosmosDb model should be called once");
        _mapperMock.Verify(mapper => mapper.Map<SiteResponseModel>(cosmosItem), Times.Once, "Map to SiteResponseModel should be called once");
    }

    [TestMethod]
    public async Task CreateSiteAsync_ThrowsSiteSvcException_WhenSiteWithSameNameExists()
    {
        // Arrange
        var siteModel = new SiteModel { Name = "ExistingSite", ChildSpaces = new List<SiteModel>() };

        _repositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None))
                       .ReturnsAsync(new List<SiteCosmosDb> { new SiteCosmosDb { Name = "ExistingSite" } });

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<SiteSvcException>(() => _spaceSvc.CreateSiteAsync(siteModel));

        Assert.AreEqual(HttpStatusCode.Conflict, exception.HttpStatusCode, "HttpStatusCode should be Conflict when site with the same name exists");
        Assert.IsTrue(exception.Message.Contains("Site with this site name already exist"), "Message should indicate that the site name is already in use");

        _repositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<SiteCosmosDb>(), CancellationToken.None), Times.Never, "CreateAsync should not be called when the site name already exists");
    }

    [TestMethod]
    public async Task CreateSiteAsync_ThrowsSiteSvcException_WhenChildSpaceHasSameNameAsSite()
    {
        // Arrange
        var siteModel = new SiteModel
        {
            Name = "DuplicateName",
            ChildSpaces = new List<SiteModel> { new SiteModel { Name = "DuplicateName" } }
        };

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<SiteSvcException>(() => _spaceSvc.CreateSiteAsync(siteModel));

        Assert.AreEqual(HttpStatusCode.Conflict, exception.HttpStatusCode, "HttpStatusCode should be Conflict when child space name matches site name");
        Assert.IsTrue(exception.Message.Contains("Site name mast be unique"), "Message should indicate that the site name must be unique among child spaces");

        _repositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<SiteCosmosDb>(), CancellationToken.None), Times.Never, "CreateAsync should not be called when child space has the same name");
    }

    [TestMethod]
    public async Task CreateSiteAsync_ThrowsSiteSvcException_WhenCosmosExceptionIsThrown()
    {
        // Arrange
        var siteModel = new SiteModel { Name = "NewSite", ChildSpaces = new List<SiteModel>() };
        var cosmosException = new CosmosException("Cosmos error", System.Net.HttpStatusCode.BadRequest, 0, "", 0);

        _repositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<SiteCosmosDb>(), CancellationToken.None))
                       .ThrowsAsync(cosmosException);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<SiteSvcException>(() => _spaceSvc.CreateSiteAsync(siteModel));

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, exception.HttpStatusCode, "HttpStatusCode should match the Cosmos exception");
        Assert.IsTrue(exception.Message.Contains("Cosmos Related exception"), "Exception message should indicate that it was a Cosmos exception");

        _repositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<SiteCosmosDb>(), CancellationToken.None), Times.Once, "CreateAsync should be called once before the exception");
        _loggerMock.Verify(logger => logger.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<CosmosException>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once, "Logger should capture the Cosmos exception");
    }

    [TestMethod]
    public async Task CreateSiteAsync_ThrowsSiteSvcException_WhenGeneralExceptionIsThrown()
    {
        // Arrange
        var siteModel = new SiteModel { Name = "NewSite", ChildSpaces = new List<SiteModel>() };

        _repositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<SiteCosmosDb>(), CancellationToken.None))
                       .ThrowsAsync(new Exception("General error"));

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<SiteSvcException>(() => _spaceSvc.CreateSiteAsync(siteModel));

        Assert.IsTrue(exception.Message.Contains("Not a cosmos Related Exception"), "Exception message should indicate it's not a Cosmos exception");

        _repositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<SiteCosmosDb>(), CancellationToken.None), Times.Once, "CreateAsync should be called once before the exception");
        _loggerMock.Verify(logger => logger.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once, "Logger should capture the general exception");
    }
}
