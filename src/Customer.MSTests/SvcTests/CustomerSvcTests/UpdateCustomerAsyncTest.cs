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
    public class UpdateCustomerAsyncTest
    {
        private Mock<IRepository<CustomerCosmosDb>> _mockCustomerCosmosRepository;
        private Mock<IRepository<UserCosmosDb>> _mockUserCosmosRepository;
        private Mock<IRepository<SiteCosmosDb>> _mockSiteCosmosRepository;
        private Mock<IMapper> _mockMapper;
        private AuthToUserProvider _authProvicer;
        private CustomerSvc _customerService;
        public UpdateCustomerAsyncTest()
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
        public async Task UpdateCustomerAsync_ExistingCustomer_ReturnsUpdatedCustomer()
        {
            // Arrange
            string customerId = "customerId";
            CustomerModel customerModel = new CustomerModel();
            CustomerResponseModel customerResponse = new CustomerResponseModel();
            CustomerCosmosDb customerCosmosDb = new CustomerCosmosDb();
            CancellationToken cancellationToken = new CancellationToken();

            _mockCustomerCosmosRepository.Setup(repo => repo.UpdateAsync(customerCosmosDb, false, cancellationToken))
                          .ReturnsAsync(customerCosmosDb);
            _mockCustomerCosmosRepository.Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<string>(), cancellationToken))
                          .ReturnsAsync(new CustomerCosmosDb());

            _mockMapper.Setup(mapper => mapper.Map<CustomerResponseModel>(customerCosmosDb))
                     .Returns(customerResponse);

            _mockMapper.Setup(mapper => mapper.Map<List<CustomerCustomerApi.Models.TenantModel>>(It.IsAny<List<CustomerCustomerApi.Models.Customer.TenantModel>>()))
                      .Returns(new List<CustomerCustomerApi.Models.TenantModel> { new CustomerCustomerApi.Models.TenantModel() });

            
            // Act
            var result = await _customerService.UpdateCustomerAsync(customerId, customerModel, cancellationToken);

            // Assert
            _mockCustomerCosmosRepository.Verify(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<string>(), cancellationToken), Times.Once);
            _mockCustomerCosmosRepository.Verify(repo => repo.UpdateAsync(It.IsAny<CustomerCosmosDb>(), false, cancellationToken), Times.Once);
            _mockMapper.Verify(mapper => mapper.Map<List<CustomerCustomerApi.Models.TenantModel>>(It.IsAny<List<CustomerCustomerApi.Models.Customer.TenantModel>>()), Times.Once);
            _mockMapper.Verify(mapper => mapper.Map<CustomerResponseModel>(It.IsAny<CustomerCosmosDb>()), Times.Once);

        }

        [TestMethod]
        [ExpectedException(typeof(CustomerSvcException))]
        public async Task UpdateCustomerAsync_NonExistingCustomer_ThrowsCustomerSvcException()
        {
            // Arrange
            string customerId = "nonExistingId";
            CustomerModel customerModel = new CustomerModel(); 
            CancellationToken cancellationToken = new CancellationToken();

            _mockCustomerCosmosRepository.Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync((CustomerCosmosDb)null); 

            // Act
            await _customerService.UpdateCustomerAsync(customerId, customerModel, cancellationToken);

            // Assert
            _mockCustomerCosmosRepository.Verify(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<string>(), cancellationToken), Times.Once);
            _mockCustomerCosmosRepository.Verify(repo => repo.UpdateAsync(It.IsAny<CustomerCosmosDb>(), false, cancellationToken), Times.Never);
            _mockMapper.Verify(mapper => mapper.Map<List<CustomerCustomerApi.Models.TenantModel>>(It.IsAny<List<CustomerCustomerApi.Models.Customer.TenantModel>>()), Times.Never);
            _mockMapper.Verify(mapper => mapper.Map<CustomerResponseModel>(It.IsAny<CustomerCosmosDb>()), Times.Never);
        }

        [TestMethod]
        [ExpectedException(typeof(CustomerSvcException))]
        public async Task UpdateCustomerAsync_CosmosException_ThrowsCustomerSvcException()
        {
            // Arrange
            string customerId = "customerId";
            CustomerModel customerModel = new CustomerModel(); 
            CancellationToken cancellationToken = new CancellationToken();

            _mockCustomerCosmosRepository.Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                          .ThrowsAsync(new CosmosException("Error message", System.Net.HttpStatusCode.InternalServerError, 0, "test", 0));

            // Act
            await _customerService.UpdateCustomerAsync(customerId, customerModel, cancellationToken);

            // Assert
            _mockCustomerCosmosRepository.Verify(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<string>(), cancellationToken), Times.Once);
            _mockCustomerCosmosRepository.Verify(repo => repo.UpdateAsync(It.IsAny<CustomerCosmosDb>(), false, cancellationToken), Times.Never);
            _mockMapper.Verify(mapper => mapper.Map<List<CustomerCustomerApi.Models.TenantModel>>(It.IsAny<List<CustomerCustomerApi.Models.Customer.TenantModel>>()), Times.Never);
            _mockMapper.Verify(mapper => mapper.Map<CustomerResponseModel>(It.IsAny<CustomerCosmosDb>()), Times.Never);
        }
    }
}
