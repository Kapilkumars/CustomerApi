using AutoMapper;
using CustomerCustomerApi.Exceptions;
using CustomerCustomerApi.Models;
using CustomerCustomerApi.Models.Module;
using CustomerCustomerApi.Services;
using Microsoft.Azure.CosmosRepository;
using Moq;

namespace CustomerCustomer.MSTests.SvcTests.ModuleSvcTests
{
    [TestClass]
    public class CreateModuleAsyncTests
    {
        [TestMethod]
        public async Task CreateModuleAsync_CreateModuleAndReturnsResponse()
        {
            // Arrange
            //Create input properties
            var cancellationToken = CancellationToken.None;
            var moduleModel = new ModuleModel();

            //Mock cosmos repository
            var moduleCosmosRepositoryMock = new Mock<IRepository<ModuleCosmosDb>>();

            var createdModuleItem = new ModuleCosmosDb();

            moduleCosmosRepositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<ModuleCosmosDb>(), cancellationToken))
                                      .ReturnsAsync(createdModuleItem);

            var createdModuleResponse = new ModuleResponse 
            {
                Id = createdModuleItem.Id
            };

            //Mock auto mapper
            var mapperMock = new Mock<IMapper>();

            mapperMock.Setup(m => m.Map<ModuleCosmosDb>(It.IsAny<ModuleModel>()))
                      .Returns(createdModuleItem);
            mapperMock.Setup(m => m.Map<ModuleResponse>(It.IsAny<ModuleCosmosDb>()))
                     .Returns(createdModuleResponse);

            // System Under Test
            var sut = new ModuleSvc(mapperMock.Object, moduleCosmosRepositoryMock.Object);

            // Act
            var result = await sut.CreateModuleAsync(moduleModel, cancellationToken);

            // Assert
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result);

            moduleCosmosRepositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<ModuleCosmosDb>(), cancellationToken), Times.Once);
            mapperMock.Verify(mapper => mapper.Map<ModuleCosmosDb>(It.IsAny<ModuleModel>()), Times.Once);
            mapperMock.Verify(mapper => mapper.Map<ModuleResponse>(It.IsAny<ModuleCosmosDb>()), Times.Once);
        }

        [TestMethod]
        public async Task CreateModuleAsync_ThrowsExceptionOnRepositoryFailure()
        {
            // Arrange
            //Create input properties
            var cancellationToken = CancellationToken.None;
            var moduleModel = new ModuleModel();

            //Mock cosmos repository
            var moduleCosmosRepositoryMock = new Mock<IRepository<ModuleCosmosDb>>();

            moduleCosmosRepositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<ModuleCosmosDb>(), cancellationToken))
                                      .ThrowsAsync(new Exception());

            //Mock auto mapper
            var mapperMock = new Mock<IMapper>();

            var createdModuleItem = new ModuleCosmosDb();

            mapperMock.Setup(m => m.Map<ModuleCosmosDb>(It.IsAny<ModuleModel>()))
                     .Returns(createdModuleItem);

            // System Under Test
            var sut = new ModuleSvc(mapperMock.Object, moduleCosmosRepositoryMock.Object);

            // Act and Assert
            await Assert.ThrowsExceptionAsync<ServiceException>(() => sut.CreateModuleAsync(moduleModel, cancellationToken));

            Assert.IsNotNull(createdModuleItem.Id);

            moduleCosmosRepositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<ModuleCosmosDb>(), cancellationToken), Times.Once);
            mapperMock.Verify(mapper => mapper.Map<ModuleCosmosDb>(It.IsAny<ModuleModel>()), Times.Once);
            mapperMock.Verify(mapper => mapper.Map<ModuleResponse>(It.IsAny<ModuleCosmosDb>()), Times.Never);
        }
    }
}
