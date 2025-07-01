using AutoMapper;
using Customer.Metis.Common.Models.CosmosDb;
using Customer.Metis.Common.Models.Responses;
using CustomerCustomerApi.Exceptions;
using CustomerCustomerApi.Services;
using Customer.Metis.Logging.Correlation;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.CosmosRepository;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using CustomerCustomerApi.Interfaces;
using CustomerCustomerApi.Models;

namespace CustomerCustomer.MSTests.SvcTests.SiteSvcTests;

[TestClass]
public class GetAllAsyncTests
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
    public async Task GetAllAsync_ReturnsMappedSiteResponseModels_WhenDataExists()
    {
        // Arrange
        var cosmosItems = new List<SiteCosmosDb> { new SiteCosmosDb(), new SiteCosmosDb() };
        _repositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None))
                       .ReturnsAsync(cosmosItems);

        var mappedItems = new List<SiteResponseModel> { new SiteResponseModel(), new SiteResponseModel() };
        _mapperMock.Setup(mapper => mapper.Map<List<SiteResponseModel>>(cosmosItems))
                   .Returns(mappedItems);

        // Act
        var result = await _siteSvc.GetAllAsync();

        // Assert
        Assert.IsNotNull(result, "Result should not be null");
        Assert.AreEqual(cosmosItems.Count, result.Count);
        _repositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None), Times.Once);
        _mapperMock.Verify(mapper => mapper.Map<List<SiteResponseModel>>(cosmosItems), Times.Once, "Map should be called exactly once");
    }

    [TestMethod]
    public async Task GetAllAsync_ThrowsSiteSvcException_WhenCosmosExceptionIsThrown()
    {
        // Arrange
        var cosmosException = new CosmosException("Cosmos error", System.Net.HttpStatusCode.BadRequest, 0, "", 0);
        _repositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None))
                       .ThrowsAsync(cosmosException);

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<SiteSvcException>(() => _siteSvc.GetAllAsync());

        Assert.IsNotNull(exception, "Exception should not be null");
        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, exception.HttpStatusCode, "HttpStatusCode should be BadRequest");
        Assert.IsTrue(exception.Message.Contains("Cosmos Related exception"), "Message should contain 'Cosmos Related exception'");

        _repositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None), Times.Once, "GetAsync should be called exactly once before exception");
        _loggerMock.Verify(logger => logger.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<CosmosException>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once, "Logger should capture the exception");
    }

    [TestMethod]
    public async Task GetAllAsync_ThrowsSiteSvcException_WhenGeneralExceptionIsThrown()
    {
        // Arrange
        _repositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None))
                       .ThrowsAsync(new Exception("General error"));

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<SiteSvcException>(() => _siteSvc.GetAllAsync());
        Assert.IsNotNull(exception, "Exception should not be null");
        Assert.IsTrue(exception.Message.Contains("Not a cosmos Related Exception"), "Message should contain 'Not a cosmos Related Exception'");

        _repositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None), Times.Once, "GetAsync should be called exactly once before exception");
        _loggerMock.Verify(logger => logger.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once, "Logger should capture the general exception");
    }
}
