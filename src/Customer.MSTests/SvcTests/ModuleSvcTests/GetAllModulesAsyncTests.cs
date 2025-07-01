using AutoMapper;
using CommonModels;
using CustomerCustomerApi.Exceptions;
using CustomerCustomerApi.Models;
using CustomerCustomerApi.Models.Module;
using CustomerCustomerApi.Services;
using Microsoft.Azure.CosmosRepository;
using Moq;
using System.Linq.Expressions;

namespace CustomerCustomer.MSTests.SvcTests.ModuleSvcTests
{
    [TestClass]
    public class GetAllModulesAsyncTests
    {
        [TestMethod]
        public async Task GetAllModulesAsync_ReturnsMappedResponseList()
        {
            // Arrange
            //Create input properties
            var cancellationToken = CancellationToken.None;

            var moduleItems = new List<ModuleCosmosDb>
            {
               new ModuleCosmosDb(),
               new ModuleCosmosDb()
            };

            //Mock cosmos repository
            var moduleCosmosRepositoryMock = new Mock<IRepository<ModuleCosmosDb>>();
            moduleCosmosRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<ModuleCosmosDb, bool>>>(), cancellationToken))
                                      .ReturnsAsync(moduleItems);

            //Mock auto mapper
            var mapperMock = new Mock<IMapper>();

            var mapperResul = new List<ModuleResponse> {
                          new ModuleResponse
                          {
                            Id = moduleItems[0].Id
                          },
                          new ModuleResponse
                          {
                            Id = moduleItems[1].Id
                          }
                      };

            mapperMock.Setup(m => m.Map<List<ModuleResponse>>(It.IsAny<List<ModuleCosmosDb>>()))
                      .Returns(mapperResul);

            // System Under Test
            var sut = new ModuleSvc(mapperMock.Object, moduleCosmosRepositoryMock.Object);

            // Act
            var result = await sut.GetAllModulesAsync(cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(List<ModuleResponse>));

            moduleCosmosRepositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<ModuleCosmosDb, bool>>>(), cancellationToken), Times.Once);
            mapperMock.Verify(m => m.Map<List<ModuleResponse>>(It.IsAny<List<ModuleCosmosDb>>()), Times.Once);
        }

        [TestMethod]
        public async Task GetAllModulesAsync_ThrowsExceptionOnRepositoryFailure()
        {
            // Arrange
            //Create input properties
            var cancellationToken = CancellationToken.None;

            //Mock cosmos repository
            var moduleCosmosRepositoryMock = new Mock<IRepository<ModuleCosmosDb>>();
            moduleCosmosRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<ModuleCosmosDb, bool>>>(), cancellationToken))
                                      .ThrowsAsync(new Exception("Repository error."));

            //Mock auto mapper
            var mapperMock = new Mock<IMapper>();
            // System Under Test
            var sut = new ModuleSvc(mapperMock.Object, moduleCosmosRepositoryMock.Object);

            // Act and Assert
            await Assert.ThrowsExceptionAsync<ServiceException>(() => sut.GetAllModulesAsync(cancellationToken));

            moduleCosmosRepositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<ModuleCosmosDb, bool>>>(), cancellationToken), Times.Once);
            mapperMock.Verify(m => m.Map<List<ModuleResponse>>(It.IsAny<List<ModuleCosmosDb>>()), Times.Never);
        }
    }
}
