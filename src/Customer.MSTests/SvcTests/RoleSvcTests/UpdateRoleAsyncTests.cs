using AutoMapper;
using CustomerCustomerApi.Exceptions;
using CustomerCustomerApi.Models;
using CustomerCustomerApi.Models.Rbac;
using CustomerCustomerApi.Services;
using Microsoft.Azure.CosmosRepository;
using Moq;
using System.Linq.Expressions;

namespace CustomerCustomer.MSTests.SvcTests.RoleSvcTests
{
    [TestClass]
    public class UpdateRoleAsyncTests
    {
        [TestMethod]
        public async Task UpdateRoleAsync_WhenRoleExists_ShouldUpdateRole()
        {
            // Arrange
            var cancellationToken = new CancellationToken();
            var roleModel = new RoleModel
            {
                Permission = new Permission
                {
                    UiActions = new List<CustomerCustomerApi.Models.Rbac.ActionInfo>(),
                    DataActions = new List<CustomerCustomerApi.Models.Rbac.ActionInfo>()
                }
            };

            var roleItem = new RoleCosmosDb
            {
                Permissions = new RolePermision
                {
                    UiActions = new List<CustomerCustomerApi.Models.ActionInfo>(),
                    DataActions = new List<CustomerCustomerApi.Models.ActionInfo>()
                }
            };

            var rolesItems = new List<RoleCosmosDb>()
            {
                roleItem
            };

            var roleCosmosRepositoryMock = new Mock<IRepository<RoleCosmosDb>>();
            var actionCosmosRepositoryMock = new Mock<IRepository<RbacActionCosmosDb>>();
            var resourceCosmosRepositoryMock = new Mock<IRepository<RbacResourceCosmosDb>>();

            roleCosmosRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<RoleCosmosDb, bool>>>(), cancellationToken))
                                     .Returns(ValueTask.FromResult<IEnumerable<RoleCosmosDb>>(rolesItems));

            roleCosmosRepositoryMock
               .Setup(repo => repo.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), cancellationToken))
               .ReturnsAsync(false);

            var mapperMock = new Mock<IMapper>();
            mapperMock
                .Setup(m => m.Map<RoleResponse>(It.IsAny<RoleCosmosDb>()))
                .Returns((RoleCosmosDb role) => new RoleResponse());

            var service = new RoleSvc(mapperMock.Object, roleCosmosRepositoryMock.Object, actionCosmosRepositoryMock.Object, resourceCosmosRepositoryMock.Object);

            // Act
            var result = await service.UpdateRoleAsync(It.IsAny<string>(), roleModel, cancellationToken);

            // Assert

            roleCosmosRepositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<RoleCosmosDb, bool>>>(), cancellationToken), Times.Once);
            roleCosmosRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<RoleCosmosDb>(), false, cancellationToken), Times.Once);
            mapperMock.Verify(m => m.Map<RoleResponse>(It.IsAny<RoleCosmosDb>()), Times.Once);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task UpdateRoleAsync_WhenRoleNotFound_ShouldThrowRoleSvcException()
        {
            // Arrange
            var roleId = Guid.NewGuid().ToString();
            var cancellationToken = new CancellationToken();
            var roleModel = new RoleModel();

            var roleCosmosRepositoryMock = new Mock<IRepository<RoleCosmosDb>>();
            var actionCosmosRepositoryMock = new Mock<IRepository<RbacActionCosmosDb>>();
            var resourceCosmosRepositoryMock = new Mock<IRepository<RbacResourceCosmosDb>>();

            roleCosmosRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<RoleCosmosDb, bool>>>(), cancellationToken))
                            .ThrowsAsync(new RBACException("Repository error."));

            var mapperMock = new Mock<IMapper>();

            var service = new RoleSvc(mapperMock.Object, roleCosmosRepositoryMock.Object, actionCosmosRepositoryMock.Object, resourceCosmosRepositoryMock.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<RBACException>(() => service.UpdateRoleAsync(It.IsAny<string>(), It.IsAny<RoleModel>(), cancellationToken));

            roleCosmosRepositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<RoleCosmosDb, bool>>>(), cancellationToken), Times.Once);
            roleCosmosRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<RoleCosmosDb>(), false, cancellationToken), Times.Never);
            mapperMock.Verify(m => m.Map<RoleResponse>(It.IsAny<RoleCosmosDb>()), Times.Never);
        }

        [TestMethod]
        public async Task UpdateRoleAsync_WhenRepositoryThrowsExceptionDuringUpdate_ShouldThrowException()
        {
            // Arrange
            var roleId = Guid.NewGuid().ToString();
            var cancellationToken = new CancellationToken();
            var roleModel = new RoleModel
            {
                Permission = new Permission
                {
                    UiActions = new List<CustomerCustomerApi.Models.Rbac.ActionInfo>(),
                    DataActions = new List<CustomerCustomerApi.Models.Rbac.ActionInfo>()
                }
            };

            var roleItem = new RoleCosmosDb
            {
                Permissions = new RolePermision
                {
                    UiActions = new List<CustomerCustomerApi.Models.ActionInfo>(),
                    DataActions = new List<CustomerCustomerApi.Models.ActionInfo>()
                }
            };

            var rolesItems = new List<RoleCosmosDb>()
            {
                roleItem
            };

            var roleCosmosRepositoryMock = new Mock<IRepository<RoleCosmosDb>>();
            var actionCosmosRepositoryMock = new Mock<IRepository<RbacActionCosmosDb>>();
            var resourceCosmosRepositoryMock = new Mock<IRepository<RbacResourceCosmosDb>>();
            roleCosmosRepositoryMock
               .Setup(repo => repo.ExistsAsync(It.IsAny<string>(), It.IsAny<string>(), cancellationToken))
               .ReturnsAsync(false);

            roleCosmosRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<RoleCosmosDb, bool>>>(), cancellationToken))
                                     .Returns(ValueTask.FromResult<IEnumerable<RoleCosmosDb>>(rolesItems));

            roleCosmosRepositoryMock
                .Setup(repo => repo.UpdateAsync(It.IsAny<RoleCosmosDb>(), false, cancellationToken))
                .ThrowsAsync(new Exception("Repository exception"));

            var mapperMock = new Mock<IMapper>();

            var service = new RoleSvc(mapperMock.Object, roleCosmosRepositoryMock.Object, actionCosmosRepositoryMock.Object, resourceCosmosRepositoryMock.Object);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<RBACException>(() => service.UpdateRoleAsync(It.IsAny<string>(), roleModel, cancellationToken));

            roleCosmosRepositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<RoleCosmosDb, bool>>>(), cancellationToken), Times.Once);
            roleCosmosRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<RoleCosmosDb>(), false, cancellationToken), Times.Once);
            mapperMock.Verify(m => m.Map<RoleResponse>(It.IsAny<RoleCosmosDb>()), Times.Never);
        }
    }
}