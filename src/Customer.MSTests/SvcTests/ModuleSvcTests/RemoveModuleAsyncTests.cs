using AutoMapper;
using CustomerCustomerApi.Exceptions;
using CustomerCustomerApi.Models;
using CustomerCustomerApi.Services;
using Microsoft.Azure.CosmosRepository;
using Moq;

namespace CustomerCustomer.MSTests.SvcTests.ModuleSvcTests
{
    [TestClass]
    public class RemoveModuleAsyncTests
    {
        [TestMethod]
        public async Task RemoveModuleAsync_RemoveResourceAndReturnsResponse()
        {
            // Arrange
            //Create input properties
            var cancellationToken = CancellationToken.None;

            var moduleItem = new ModuleCosmosDb
            {
                IsDeleted = false,
            };

            //Mock cosmos repository
            var moduleCosmosRepositoryMock = new Mock<IRepository<ModuleCosmosDb>>();
            moduleCosmosRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<string>(), cancellationToken))
                                      .ReturnsAsync(moduleItem);
            //Mock auto mapper
            var mapperMock = new Mock<IMapper>();

            // System Under Test
            var sut = new ModuleSvc(mapperMock.Object, moduleCosmosRepositoryMock.Object);

            // Act
            await sut.RemoveModuleAsync(moduleItem.Id, cancellationToken);

            // Assert
            moduleCosmosRepositoryMock.Verify(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<string>(), cancellationToken), Times.Once);
            moduleCosmosRepositoryMock.Verify(repo => repo.UpdateAsync(moduleItem, false, cancellationToken), Times.Once);
        }

        [TestMethod]
        public async Task RemoveModuleAsync_ThrowsExceptionOnRepositoryFailure()
        {
            // Arrange
            //Create input properties
            var cancellationToken = CancellationToken.None;

            //Mock cosmos repository
            var moduleCosmosRepositoryMock = new Mock<IRepository<ModuleCosmosDb>>();
            var createdResourceItem = new ModuleCosmosDb();

            moduleCosmosRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<string>(), cancellationToken))
                                      .ReturnsAsync(createdResourceItem);
            moduleCosmosRepositoryMock.Setup(repo => repo.UpdateAsync(createdResourceItem, false, cancellationToken))
                                      .ThrowsAsync(new Exception("Repository error."));

            //Mock auto mapper
            var mapperMock = new Mock<IMapper>();

            // System Under Test
            var sut = new ModuleSvc(mapperMock.Object, moduleCosmosRepositoryMock.Object);

            // Act and Assert
            await Assert.ThrowsExceptionAsync<ServiceException>(() => sut.RemoveModuleAsync(It.IsAny<string>(), cancellationToken));

            moduleCosmosRepositoryMock.Verify(repo => repo.GetAsync(It.IsAny<string>(), It.IsAny<string>(), cancellationToken), Times.Once);
            moduleCosmosRepositoryMock.Verify(repo => repo.UpdateAsync(createdResourceItem, false, cancellationToken), Times.Once);
        }
    }
}
