using AutoMapper;
using CommonModels;
using CustomerCustomerApi.Exceptions;
using CustomerCustomerApi.Models;
using CustomerCustomerApi.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.CosmosRepository;
using Moq;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace CustomerCustomer.MSTests.SvcTests.RoleSvcTests
{
    [TestClass]
    public class RemoveRoleAsyncTests
    {
        [TestMethod]
        public async Task RemoveRoleAsync_RemoveRoleAndReturnsResponse()
        {
            // Arrange
            //Create input properties
            var cancellationToken = CancellationToken.None;
            var roleId = "test-role-id";

            var createdRoleItems = new List<RoleCosmosDb>
            {
                new RoleCosmosDb
                {
                }
            };

            //Mock cosmos repository
            var actionCosmosRepositoryMock = new Mock<IRepository<RbacActionCosmosDb>>();
            var resourceCosmosRepositoryMock = new Mock<IRepository<RbacResourceCosmosDb>>();
            var roleCosmosRepositoryMock = new Mock<IRepository<RoleCosmosDb>>();
            //Mock auto mapper
            var mapperMock = new Mock<IMapper>();

            var res = new List<RoleCosmosDb>() { new RoleCosmosDb() };

            roleCosmosRepositoryMock.Setup(repo => repo.DeleteAsync(roleId, null, cancellationToken))
                                    .Returns(ValueTask.CompletedTask);

            // System Under Test
            var sut = new RoleSvc(mapperMock.Object, roleCosmosRepositoryMock.Object, actionCosmosRepositoryMock.Object, resourceCosmosRepositoryMock.Object);

            // Act
            await sut.RemoveRoleAsync(roleId, cancellationToken);

            // Assert
            roleCosmosRepositoryMock.Verify(repo => repo.DeleteAsync(roleId, null, cancellationToken), Times.Once);
        }

        [TestMethod]
        public async Task RemoveRoleAsync_ThrowsExceptionOnRepositoryFailure()
        {
            // Arrange
            //Create input properties
            var cancellationToken = CancellationToken.None;
            var roleId = "test-role-id";

            //Mock cosmos repository
            var actionCosmosRepositoryMock = new Mock<IRepository<RbacActionCosmosDb>>();
            var resourceCosmosRepositoryMock = new Mock<IRepository<RbacResourceCosmosDb>>();
            var roleCosmosRepositoryMock = new Mock<IRepository<RoleCosmosDb>>();

            var res = new List<RoleCosmosDb>() { new RoleCosmosDb() };


            roleCosmosRepositoryMock.Setup(repo => repo.DeleteAsync(roleId, null, cancellationToken))
                                    .ThrowsAsync(new Exception("Repository error."));

            //Mock auto mapper
            var mapperMock = new Mock<IMapper>();

            // System Under Test
            var sut = new RoleSvc(mapperMock.Object, roleCosmosRepositoryMock.Object, actionCosmosRepositoryMock.Object, resourceCosmosRepositoryMock.Object);

            // Act and Assert
            var ex = await Assert.ThrowsExceptionAsync<RBACException>(() => sut.RemoveRoleAsync(roleId, cancellationToken));
            Assert.AreEqual("Not cosmos related exception, see inner exception!", ex.Message);

            roleCosmosRepositoryMock.Verify(repo => repo.DeleteAsync(roleId, null, cancellationToken), Times.Once);
        }
    }
}

