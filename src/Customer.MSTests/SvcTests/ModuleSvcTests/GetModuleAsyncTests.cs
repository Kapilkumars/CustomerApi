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
    public class GetModuleAsyncTests
    {
        [TestMethod]
        public async Task GetAllModulesAsync_ReturnsMappedResponseList()
        {
            // Arrange
            //Create input properties
            var cancellationToken = CancellationToken.None;

            var moduleItem = new ModuleCosmosDb();

            //Mock cosmos repository
            var moduleCosmosRepositoryMock = new Mock<IRepository<ModuleCosmosDb>>();
            moduleCosmosRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<string>(), cancellationToken))
                                      .ReturnsAsync(moduleItem);

            //Mock auto mapper
            var mapperMock = new Mock<IMapper>();

            var mapperResul = new ModuleResponse { Id = moduleItem.Id };

            mapperMock.Setup(m => m.Map<ModuleResponse>(It.IsAny<ModuleCosmosDb>()))
                      .Returns(mapperResul);

            // System Under Test
            var sut = new ModuleSvc(mapperMock.Object, moduleCosmosRepositoryMock.Object);

            // Act
            var result = await sut.GetModuleAsync(moduleItem.Id, cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ModuleResponse));

            moduleCosmosRepositoryMock.Verify(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<string>(), cancellationToken), Times.Once);
            mapperMock.Verify(m => m.Map<ModuleResponse>(It.IsAny<ModuleCosmosDb>()), Times.Once);
        }

        [TestMethod]
        public async Task GetAllModulesAsync_ThrowsExceptionOnRepositoryFailure()
        {
            // Arrange
            //Create input properties
            var cancellationToken = CancellationToken.None;

            //Mock cosmos repository
            var moduleCosmosRepositoryMock = new Mock<IRepository<ModuleCosmosDb>>();
            moduleCosmosRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<string>(), cancellationToken))
                                      .ThrowsAsync(new Exception("Repository error."));

            //Mock auto mapper
            var mapperMock = new Mock<IMapper>();
            // System Under Test
            var sut = new ModuleSvc(mapperMock.Object, moduleCosmosRepositoryMock.Object);

            // Act and Assert
            await Assert.ThrowsExceptionAsync<ServiceException>(() => sut.GetModuleAsync(It.IsAny<string>(), cancellationToken));

            moduleCosmosRepositoryMock.Verify(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<string>(), cancellationToken), Times.Once);
            mapperMock.Verify(m => m.Map<ModuleResponse>(It.IsAny<ModuleCosmosDb>()), Times.Never);
        }
    }
}
