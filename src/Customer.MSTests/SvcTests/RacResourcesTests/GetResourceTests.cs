using AutoMapper;
using CustomerCustomerApi.Models;
using CustomerCustomerApi.Models.Rbac;
using CustomerCustomerApi.Services;
using Microsoft.Azure.CosmosRepository;
using Moq;
using System.Linq.Expressions;

namespace CustomerCustomer.MSTests.SvcTests.RacResourcesTests
{
    [TestClass]
    public class GetResourceTests
    {
        [TestMethod]
        public async Task GetAllResourcesAsync_ReturnsMappedResponseList()
        {
            // Arrange
            //Create input properties
            var cancellationToken = CancellationToken.None;

            var resourceItems = new List<RbacResourceCosmosDb>
            {
                new RbacResourceCosmosDb {
                    Description = "test description",
                    ResourceName = "test",
                },
               new RbacResourceCosmosDb {
                    Description = "test description",
                    ResourceName = "test",
                }
            };

            //Mock cosmos repository
            var resourceCosmosRepositoryMock = new Mock<IRepository<RbacResourceCosmosDb>>();
            resourceCosmosRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<RbacResourceCosmosDb, bool>>>(), cancellationToken))
                                      .ReturnsAsync(resourceItems);

            //Mock auto mapper
            var mapperMock = new Mock<IMapper>();

            var mapperResul = new List<RbacResourceResponse> {
                          new RbacResourceResponse
                          {
                            Id = resourceItems[0].Id,
                            Description = "tes description",
                            ResourceName = "test",
                          },
                          new RbacResourceResponse
                          {
                            Id = resourceItems[1].Id,
                            Description = "test description",
                            ResourceName = "test",
                          }
                      };

            mapperMock.Setup(m => m.Map<List<RbacResourceResponse>>(It.IsAny<List<RbacResourceCosmosDb>>()))
                      .Returns(mapperResul);

            // System Under Test
            var sut = new RbacResourceSvc(mapperMock.Object, resourceCosmosRepositoryMock.Object);

            // Act
            var result = await sut.GetAllResourcesAsync(cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(List<RbacResourceResponse>));

            resourceCosmosRepositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<RbacResourceCosmosDb, bool>>>(), cancellationToken), Times.Once);
            mapperMock.Verify(m => m.Map<List<RbacResourceResponse>>(It.IsAny<List<RbacResourceCosmosDb>>()), Times.Once);
        }

        [TestMethod]
        public async Task GetAllResourcesAsync_ThrowsExceptionOnRepositoryFailure()
        {
            // Arrange
            //Create input properties
            var cancellationToken = CancellationToken.None;

            //Mock cosmos repository
            var resourceCosmosRepositoryMock = new Mock<IRepository<RbacResourceCosmosDb>>();
            resourceCosmosRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<RbacResourceCosmosDb, bool>>>(), cancellationToken))
                                      .ThrowsAsync(new Exception("Repository error."));

            //Mock auto mapper
            var mapperMock = new Mock<IMapper>();
            // System Under Test
            var sut = new RbacResourceSvc(mapperMock.Object, resourceCosmosRepositoryMock.Object);

            // Act and Assert
            await Assert.ThrowsExceptionAsync<Exception>(() => sut.GetAllResourcesAsync(cancellationToken));

            resourceCosmosRepositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<RbacResourceCosmosDb, bool>>>(), cancellationToken), Times.Once);
            mapperMock.Verify(m => m.Map<List<RbacResourceResponse>>(It.IsAny<List<RbacResourceCosmosDb>>()), Times.Never);
        }
    }
}
