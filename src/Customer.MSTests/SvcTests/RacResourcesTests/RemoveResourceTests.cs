using AutoMapper;
using CustomerCustomerApi.Exceptions;
using CustomerCustomerApi.Models;
using CustomerCustomerApi.Services;
using Microsoft.Azure.CosmosRepository;
using Moq;
using System.Linq.Expressions;

namespace CustomerCustomer.MSTests.SvcTests.RacResourcesTests
{
    [TestClass]
    public class RemoveResourceTests
    {
        [TestMethod]
        public async Task RemoveResourceAsync_RemoveResourceAndReturnsResponse()
        {
            // Arrange
            //Create input properties
            var cancellationToken = CancellationToken.None;

            var createdResourceItem = new List<RbacResourceCosmosDb>()
            {
                new RbacResourceCosmosDb
                {
                    Id = "123",
                    ResourceName = "tset",
                    Description = "some description"
                }
            };


            //Mock cosmos repository
            var resourceCosmosRepositoryMock = new Mock<IRepository<RbacResourceCosmosDb>>();

            resourceCosmosRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<RbacResourceCosmosDb, bool>>>(), cancellationToken))
                .Returns(ValueTask.FromResult<IEnumerable<RbacResourceCosmosDb>>(createdResourceItem));
            //Mock auto mapper
            var mapperMock = new Mock<IMapper>();

            // System Under Test
            var sut = new RbacResourceSvc(mapperMock.Object, resourceCosmosRepositoryMock.Object);

            // Act
            await sut.RemoveResourceAsync(createdResourceItem.First().Id, cancellationToken);

            // Assert
            resourceCosmosRepositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<RbacResourceCosmosDb, bool>>>(), cancellationToken), Times.Once);
            resourceCosmosRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<RbacResourceCosmosDb>(r => r.IsDeleted), false, cancellationToken), Times.Once);
        }

        [TestMethod]
        public async Task RemoveResourceAsync_ThrowsExceptionOnRepositoryFailure()
        {
            // Arrange
            //Create input properties
            var cancellationToken = CancellationToken.None;

            //Mock cosmos repository
            var resourceCosmosRepositoryMock = new Mock<IRepository<RbacResourceCosmosDb>>();
            var createdResourceItem = new List<RbacResourceCosmosDb>()
                {
                    new RbacResourceCosmosDb
                    {
                        Id = "123",
                        ResourceName = "tset",
                        Description = "some description"
                    }
                };

            resourceCosmosRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<RbacResourceCosmosDb, bool>>>(), cancellationToken))
                                   .Returns(ValueTask.FromResult<IEnumerable<RbacResourceCosmosDb>>(createdResourceItem));

            resourceCosmosRepositoryMock.Setup(repo => repo.UpdateAsync(createdResourceItem.First(), false, cancellationToken))
                .ThrowsAsync(new Exception("Repository error."));

            //Mock auto mapper
            var mapperMock = new Mock<IMapper>();

            // System Under Test
            var sut = new RbacResourceSvc(mapperMock.Object, resourceCosmosRepositoryMock.Object);

            // Act and Assert
            await Assert.ThrowsExceptionAsync<RBACException>(() => sut.RemoveResourceAsync("123", cancellationToken));

            resourceCosmosRepositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<RbacResourceCosmosDb, bool>>>(), cancellationToken), Times.Once);
            resourceCosmosRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<RbacResourceCosmosDb>(r => r.Id == "123" && r.IsDeleted), false, cancellationToken), Times.Once);
        }
    }
}
