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
    public class UpdateModuleAsyncTests
    {
        [TestMethod]
        public async Task UpdateModuleAsync_WhenModuleExists_ShouldUpdateModule()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var moduleModel = new ModuleModel 
            {
                Name = "Test",
                Description = "Test",
                Cost = 1.1,
                IsSubscription = true
            };
            var moduleItem = new ModuleCosmosDb();
            var moduleResponse = new ModuleResponse { Id = moduleItem.Id};

            var moduleCosmosRepositoryMock = new Mock<IRepository<ModuleCosmosDb>>();
            moduleCosmosRepositoryMock
                .Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<string>(), cancellationToken))
                .ReturnsAsync(moduleItem);

            var mapperMock = new Mock<IMapper>();
            mapperMock.Setup(m => m.Map<ModuleResponse>(It.IsAny<ModuleCosmosDb>()))
                .Returns(moduleResponse);

            var service = new ModuleSvc(mapperMock.Object, moduleCosmosRepositoryMock.Object);

            // Act
            var result = await service.UpdateModuleAsync(It.IsAny<string>(), moduleModel, cancellationToken);

            // Assert

            moduleCosmosRepositoryMock.Verify(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<string>(), cancellationToken), Times.Once);
            moduleCosmosRepositoryMock.Verify(repo => repo.UpdateAsync(moduleItem, false, cancellationToken), Times.Once);
            mapperMock.Verify(m => m.Map<ModuleResponse>(It.IsAny<ModuleCosmosDb>()), Times.Once);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task UpdateModuleAsync_WhenRepositoryThrowsExceptionDuringUpdate_ShouldThrowException()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var moduleModel = new ModuleModel();

            var moduleItem = new ModuleCosmosDb();

            var moduleCosmosRepositoryMock = new Mock<IRepository<ModuleCosmosDb>>();

            moduleCosmosRepositoryMock
               .Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<string>(), cancellationToken))
               .ReturnsAsync(moduleItem);

            moduleCosmosRepositoryMock
                .Setup(repo => repo.UpdateAsync(It.IsAny<ModuleCosmosDb>(), false, cancellationToken))
                .ThrowsAsync(new Exception("Repository exception"));

            var mapperMock = new Mock<IMapper>();

            var service = new ModuleSvc(mapperMock.Object, moduleCosmosRepositoryMock.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ServiceException>(() => service.UpdateModuleAsync(It.IsAny<string>(), moduleModel, cancellationToken));

            moduleCosmosRepositoryMock.Verify(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<string>(), cancellationToken), Times.Once);
            moduleCosmosRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<ModuleCosmosDb>(), false, cancellationToken), Times.Once);
            mapperMock.Verify(m => m.Map<ModuleResponse>(It.IsAny<ModuleCosmosDb>()), Times.Never);
        }

        [TestMethod]
        public async Task UpdateModuleAsync_WhenRepositoryReturnNull_ShouldThrowException()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var moduleModel = new ModuleModel();

            var moduleCosmosRepositoryMock = new Mock<IRepository<ModuleCosmosDb>>();

            moduleCosmosRepositoryMock
               .Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<string>(), cancellationToken))
               .ReturnsAsync(value : null);

            var mapperMock = new Mock<IMapper>();

            var service = new ModuleSvc(mapperMock.Object, moduleCosmosRepositoryMock.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ServiceException>(() => service.UpdateModuleAsync(It.IsAny<string>(), moduleModel, cancellationToken));

            moduleCosmosRepositoryMock.Verify(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<string>(), cancellationToken), Times.Once);
            moduleCosmosRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<ModuleCosmosDb>(), false, cancellationToken), Times.Never);
            mapperMock.Verify(m => m.Map<ModuleResponse>(It.IsAny<ModuleCosmosDb>()), Times.Never);
        }
    }
}
