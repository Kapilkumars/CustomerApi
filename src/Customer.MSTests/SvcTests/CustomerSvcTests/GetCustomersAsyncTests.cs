using AutoMapper;
using CustomerCustomerApi.Exceptions;
using CustomerCustomerApi.Models;
using CustomerCustomerApi.Models.Customer;
using CustomerCustomerApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.CosmosRepository;
using Moq;
using System.Linq.Expressions;

namespace CustomerCustomer.MSTests.SvcTests.CustomerSvcTests
{
    [TestClass]
    public class GetCustomersAsyncTests
    {
        private Mock<IRepository<CustomerCosmosDb>> _mockCustomerCosmosRepository;
        private Mock<IRepository<UserCosmosDb>> _mockUserCosmosRepository;
        private Mock<IRepository<SiteCosmosDb>> _mockSiteCosmosRepository;
        private Mock<IMapper> _mockMapper;
        private AuthToUserProvider _authProvicer;
        private CustomerSvc _customerService;
        public GetCustomersAsyncTests()
        {
            _mockCustomerCosmosRepository = new Mock<IRepository<CustomerCosmosDb>>();
            _mockUserCosmosRepository = new Mock<IRepository<UserCosmosDb>>();
            _mockSiteCosmosRepository = new Mock<IRepository<SiteCosmosDb>>();
            _mockMapper = new Mock<IMapper>();
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _authProvicer = new AuthToUserProvider(mockHttpContextAccessor.Object, _mockUserCosmosRepository.Object);
            _customerService = new CustomerSvc(_mockMapper.Object, _mockCustomerCosmosRepository.Object, _mockUserCosmosRepository.Object, _mockSiteCosmosRepository.Object, _authProvicer);
        }

        [TestMethod]
        public async Task GetCustomerByIdAsync_ValidInput_ReturnsCustomerResponseModels()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var customerNumber = "123456";

            var customerCosmosDbList = new List<CustomerCosmosDb> { new CustomerCosmosDb(), new CustomerCosmosDb() };
            var customerResponseModelList = new CustomerResponseModel();

            _mockCustomerCosmosRepository.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<CustomerCosmosDb, bool>>>(), cancellationToken))
                .ReturnsAsync(customerCosmosDbList);

            _mockMapper.Setup(mapper => mapper.Map<CustomerResponseModel>(customerCosmosDbList.First()))
                .Returns(customerResponseModelList);

            // Act
            var result = await _customerService.GetCustomerByIdAsync(customerNumber, cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            _mockMapper.Verify(mapper => mapper.Map<CustomerResponseModel>(It.IsAny<CustomerCosmosDb>()), Times.Once);
            _mockCustomerCosmosRepository.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<CustomerCosmosDb, bool>>>(), cancellationToken), Times.Once);
        }

        [TestMethod]
        public async Task GetCustomersByNumberAsync_CosmosRepositoryThrowsException_ThrowsCustomerSvcExceptionWithInternalServerError()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var customerNumber = "123456";

            var cosmosException = new CosmosException("Message", System.Net.HttpStatusCode.InternalServerError, 0, "ActivityId", 0);
            _mockCustomerCosmosRepository.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<CustomerCosmosDb, bool>>>(), cancellationToken))
                .ThrowsAsync(cosmosException);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<CustomerSvcException>(
                async () => await _customerService.GetCustomerByIdAsync(customerNumber, cancellationToken),
                $"Unable to get customers from database with customerNumber: {customerNumber}");
            _mockMapper.Verify(mapper => mapper.Map<List<CustomerResponseModel>>(It.IsAny<List<CustomerCosmosDb>>()), Times.Never);
            _mockCustomerCosmosRepository.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<CustomerCosmosDb, bool>>>(), cancellationToken), Times.Once);
        }

        [TestMethod]
        public async Task GetCustomersByNumberAsync_UnexpectedException_ThrowsCustomerSvcExceptionWithInternalServerError()
        {
            // Arrange
            var cancellationToken = CancellationToken.None;
            var customerNumber = "123456";

            _mockCustomerCosmosRepository.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<CustomerCosmosDb, bool>>>(), cancellationToken))
                .ThrowsAsync(new Exception("Unexpected Exception"));

            // Act & Assert
            await Assert.ThrowsExceptionAsync<CustomerSvcException>(
                async () => await _customerService.GetCustomerByIdAsync(customerNumber, cancellationToken),
                "An unexpected error occurred while obtaining customers.");
            _mockMapper.Verify(mapper => mapper.Map<List<CustomerResponseModel>>(It.IsAny<List<CustomerCosmosDb>>()), Times.Never);
            _mockCustomerCosmosRepository.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<CustomerCosmosDb, bool>>>(), cancellationToken), Times.Once);
        }

    }
}