using AutoMapper;
using CustomerCustomerApi.Models;
using CustomerCustomerApi.Models.Rbac;
using CustomerCustomerApi.Services;
using Microsoft.Azure.CosmosRepository;
using Moq;

namespace CustomerCustomer.MSTests.SvcTests.RacResourcesTests
{
    [TestClass]
    public class CreateRbacResourceTests
    {
        [TestMethod]
        public async Task CreateResourceAsync_CreatesResourceAndReturnsResponse()
        {
            // Arrange
            //Create input properties
            var cancellationToken = CancellationToken.None;
            var resourceModel = new RbacResourceModel
            {
                ResourceName = "test",
                Description = "some description"
            };

            //Mock cosmos repository
            var resourceCosmosRepositoryMock = new Mock<IRepository<RbacResourceCosmosDb>>();

            var createdResourceItem = new RbacResourceCosmosDb
            {
                ResourceName = resourceModel.ResourceName,
                Description = resourceModel.Description
            };

            resourceCosmosRepositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<RbacResourceCosmosDb>(), cancellationToken))
                                      .ReturnsAsync(createdResourceItem);

            var createdResourceResponse = new RbacResourceResponse
            {
                Id = createdResourceItem.Id,
                ResourceName = createdResourceItem.ResourceName,
                Description = createdResourceItem.Description
            };

            //Mock auto mapper
            var mapperMock = new Mock<IMapper>();

            mapperMock.Setup(m => m.Map<RbacResourceCosmosDb>(It.IsAny<RbacResourceModel>()))
                      .Returns(createdResourceItem);
            mapperMock.Setup(m => m.Map<RbacResourceResponse>(It.IsAny<RbacResourceCosmosDb>()))
                     .Returns(createdResourceResponse);

            // System Under Test
            var sut = new RbacResourceSvc(mapperMock.Object, resourceCosmosRepositoryMock.Object);

            // Act
            var result = await sut.CreateResourceAsync(resourceModel, cancellationToken);

            // Assert
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result);

            resourceCosmosRepositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<RbacResourceCosmosDb>(), cancellationToken), Times.Once);
            mapperMock.Verify(mapper => mapper.Map<RbacResourceCosmosDb>(It.IsAny<RbacResourceModel>()), Times.Once);
            mapperMock.Verify(mapper => mapper.Map<RbacResourceResponse>(It.IsAny<RbacResourceCosmosDb>()), Times.Once);
        }

        [TestMethod]
        public async Task CreateResourceAsync_ThrowsExceptionOnRepositoryFailure()
        {
            // Arrange
            //Create input properties
            var cancellationToken = CancellationToken.None;
            var resourceModel = new RbacResourceModel
            {
                ResourceName = "test",
                Description = "some description"
            };

            //Mock cosmos repository
            var resourceCosmosRepositoryMock = new Mock<IRepository<RbacResourceCosmosDb>>();

            resourceCosmosRepositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<RbacResourceCosmosDb>(), cancellationToken))
                                      .ThrowsAsync(new Exception());

            //Mock auto mapper
            var mapperMock = new Mock<IMapper>();

            var createdResourceItem = new RbacResourceCosmosDb
            {
                ResourceName = "test",
                Description = resourceModel.Description
            };

            mapperMock.Setup(m => m.Map<RbacResourceCosmosDb>(It.IsAny<RbacResourceModel>()))
                     .Returns(createdResourceItem);

            // System Under Test
            var sut = new RbacResourceSvc(mapperMock.Object, resourceCosmosRepositoryMock.Object);

            // Act and Assert
            await Assert.ThrowsExceptionAsync<Exception>(() => sut.CreateResourceAsync(resourceModel, cancellationToken));

            Assert.IsNotNull(createdResourceItem.Id);

            resourceCosmosRepositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<RbacResourceCosmosDb>(), cancellationToken), Times.Once);
        }
    }
}
