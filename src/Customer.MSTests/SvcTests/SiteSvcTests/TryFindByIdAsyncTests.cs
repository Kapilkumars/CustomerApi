using AutoMapper;
using CustomerCustomerApi.Models;
using Customer.Metis.Common.Models.Responses;
using Customer.Metis.Logging.Correlation;
using CustomerCustomerApi.Exceptions;
using CustomerCustomerApi.Interfaces;
using CustomerCustomerApi.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.CosmosRepository;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace CustomerCustomer.MSTests.SvcTests.SiteSvcTests;

[TestClass]
public class TryFindByIdAsyncTests
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
    public async Task TryFindByIdAsync_ReturnsMappedSiteResponseModel_WhenItemExists()
    {
        // Arrange
        var id = "123";
        var cosmosItem = new SiteCosmosDb { Id = id };
        _repositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None))
                        .ReturnsAsync(new List<SiteCosmosDb> { cosmosItem });

        var mappedItem = new SiteResponseModel { Id = id };
        _mapperMock.Setup(mapper => mapper.Map<SiteResponseModel>(cosmosItem))
            .Returns(mappedItem);

        // Act
        var result = await _siteSvc.TryFindByIdAsync(id);

        // Assert
        Assert.IsNotNull(result, "Result should not be null when the item exists");
        Assert.AreEqual(mappedItem.Id, result?.Id, "Mapped item SiteId should match the expected value");
        _repositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None), Times.Once, "GetAsync should be called exactly once");
        _mapperMock.Verify(mapper => mapper.Map<SiteResponseModel>(cosmosItem), Times.Once, "Map should be called exactly once");
    }

    [TestMethod]
    public async Task TryFindByIdAsync_ReturnsNull_WhenItemDoesNotExist()
    {
        // Arrange
        var id = "123";
        _repositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None))
                       .ReturnsAsync(new List<SiteCosmosDb>());

        // Act
        var result = await _siteSvc.TryFindByIdAsync(id);

        // Assert
        Assert.IsNull(result, "Result should be null when the item does not exist");
        _repositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None), Times.Once, "GetAsync should be called exactly once");
        _mapperMock.Verify(mapper => mapper.Map<SiteResponseModel>(It.IsAny<SiteCosmosDb>()), Times.Never, "Map should not be called when no item is found");
    }

    [TestMethod]
    public async Task TryFindByIdAsync_ThrowsException_WhenIdIsNullOrEmpty()
    {
        // Arrange
        var id = "";

        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<SiteSvcException>(() => _siteSvc.TryFindByIdAsync(id));
        Assert.IsTrue(exception.InnerException?.Message.Contains("Site id cant be null or empty") ?? false);

        _repositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), It.IsAny<CancellationToken>()), Times.Never, "GetAsync should not be called when id is null or empty");
    }

    [TestMethod]
    public async Task TryFindByIdAsync_ThrowsSiteSvcException_WhenCosmosExceptionIsThrown()
    {
        // Arrange
        var id = "123";
        var cosmosException = new CosmosException("Cosmos error", System.Net.HttpStatusCode.BadRequest, 0, "", 0);

        _repositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None))
                        .ThrowsAsync(cosmosException);

        _correlationIdGeneratorMock.Setup(c => c.Get()).Returns("correlation-xyz");
        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<SiteSvcException>(() => _siteSvc.TryFindByIdAsync(id));

        Assert.AreEqual(System.Net.HttpStatusCode.BadRequest, exception.HttpStatusCode, "HttpStatusCode should match the Cosmos exception");
        Assert.IsTrue(exception.Message.Contains("Cosmos Related exception"), "Exception message should contain 'Cosmos Related exception'");
        _repositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None), Times.Once, "GetAsync should be called exactly once before the exception");
        _loggerMock.Verify(logger => logger.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<CosmosException>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once, "Logger should capture the exception");
    }

    [TestMethod]
    public async Task TryFindByIdAsync_ThrowsSiteSvcException_WhenGeneralExceptionIsThrown()
    {
        // Arrange
        var id = "123";
        _repositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None))
            .ThrowsAsync(new Exception("General error"));

        _correlationIdGeneratorMock.Setup(c => c.Get()).Returns("correlation-abc");
        // Act & Assert
        var exception = await Assert.ThrowsExceptionAsync<SiteSvcException>(() => _siteSvc.TryFindByIdAsync(id));
        Assert.IsTrue(exception.Message.Contains("Not a cosmos Related Exception"), "Message should contain 'Not a cosmos Related Exception'");
        _repositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<SiteCosmosDb, bool>>>(), CancellationToken.None), Times.Once, "GetAsync should be called exactly once before the exception");
        _loggerMock.Verify(logger => logger.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()!), Times.Once, "Logger should capture the general exception");
    }
}