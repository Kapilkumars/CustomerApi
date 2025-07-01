using AutoMapper;
using CustomerCustomerApi.Exceptions;
using CustomerCustomerApi.Models;
using CustomerCustomerApi.Models.Customer;
using CustomerCustomerApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.CosmosRepository;
using Moq;

namespace CustomerCustomer.MSTests.SvcTests.CustomerSvcTests
{
    [TestClass]
    public class CreateCustomerAsyncTests
    {
        private Mock<IRepository<CustomerCosmosDb>> _mockCustomerCosmosRepository;
        private Mock<IRepository<UserCosmosDb>> _mockUserCosmosRepository;
        private Mock<IRepository<SiteCosmosDb>> _mockSiteCosmosRepository;
        private Mock<IMapper> _mockMapper;
        private AuthToUserProvider _authProvicer;
        private CustomerSvc _customerService;
        public CreateCustomerAsyncTests()
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
        public async Task CreateCustomerAsync_CreatesCustomerAndReturnsResponse()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var createCustomerModel = new CreateCustomerModel();
            var customerCosmosDb = new CustomerCosmosDb();
            var customerResponseModel = new CustomerResponseModel();

            _mockMapper.Setup(m => m.Map<CustomerCosmosDb>(createCustomerModel)).Returns(customerCosmosDb);
            _mockCustomerCosmosRepository.Setup(repo => repo.CreateAsync(customerCosmosDb, cancellationToken)).ReturnsAsync(customerCosmosDb);
            _mockMapper.Setup(m => m.Map<CustomerResponseModel>(customerCosmosDb)).Returns(customerResponseModel);

            // Act
            var result = await _customerService.CreateCustomerAsync(createCustomerModel, cancellationToken);

            // Assert
            _mockCustomerCosmosRepository.Verify(repo => repo.CreateAsync(It.IsAny<CustomerCosmosDb>(), cancellationToken), Times.Once);
            _mockMapper.Verify(mapper => mapper.Map<CustomerCosmosDb>(It.IsAny<CreateCustomerModel>()), Times.Once);
            _mockMapper.Verify(mapper => mapper.Map<CustomerResponseModel>(It.IsAny<CustomerCosmosDb>()), Times.Once);
            Assert.AreEqual(customerResponseModel, result);
        }

        [TestMethod]
        public async Task CreateCustomerAsync_CosmosException_ThrowsCustomerSvcException()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var createCustomerModel = new CreateCustomerModel();
            var cosmosException = new CosmosException("Message", System.Net.HttpStatusCode.InternalServerError, 0, "ActivityId", 0);
            
            _mockMapper.Setup(m => m.Map<CustomerCosmosDb>(createCustomerModel)).Returns(new CustomerCosmosDb());
            _mockCustomerCosmosRepository.Setup(repo => repo.CreateAsync(It.IsAny<CustomerCosmosDb>(), cancellationToken))
                                                .ThrowsAsync(cosmosException);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<CustomerSvcException>(() => _customerService.CreateCustomerAsync(createCustomerModel, cancellationToken));
            _mockCustomerCosmosRepository.Verify(repo => repo.CreateAsync(It.IsAny<CustomerCosmosDb>(), cancellationToken), Times.Once);
            _mockMapper.Verify(mapper => mapper.Map<CustomerCosmosDb>(It.IsAny<CreateCustomerModel>()), Times.Once);
            _mockMapper.Verify(mapper => mapper.Map<CustomerResponseModel>(It.IsAny<CustomerCosmosDb>()), Times.Never);
        }
    }
}
