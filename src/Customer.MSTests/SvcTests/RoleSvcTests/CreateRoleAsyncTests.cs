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
    public class CreateRoleAsyncTests
    {
        private Permission _permissions;
        private RoleModel _roleModel;
        public CreateRoleAsyncTests()
        {
            _permissions = new Permission
            {
                UiActions = new List<CustomerCustomerApi.Models.Rbac.ActionInfo>
                {
                    
                },
                DataActions = new List<CustomerCustomerApi.Models.Rbac.ActionInfo>
                {

                }
            };

            _roleModel = new RoleModel
            {
                Name = "testName",
                Properties = new Properties
                {
                    DisplayName = "Name",
                    Description = "test description",
                    Type = "test"
                },
                Permission = _permissions
            };
        }

        [TestMethod]
        public async Task CreateRoleAsync_CreatesRoleAndReturnsResponse()
        {
            // Arrange
            //Create input properties
            var cancellationToken = CancellationToken.None;

            //Mock cosmos repository
            var roleCosmosRepositoryMock = new Mock<IRepository<RoleCosmosDb>>();
            var actionCosmosRepositoryMock = new Mock<IRepository<RbacActionCosmosDb>>();
            var resourceCosmosRepositoryMock = new Mock<IRepository<RbacResourceCosmosDb>>();

            roleCosmosRepositoryMock.Setup(repo => repo.ExistsAsync(It.IsAny<Expression<Func<RoleCosmosDb, bool>>>(), cancellationToken))
                                      .ReturnsAsync(false);

            var roleItem = new RoleCosmosDb
            {
                Name = _roleModel.Name,
                Properties = new RoleProperties(),
                Permissions = new RolePermision()
            };

            roleCosmosRepositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<RoleCosmosDb>(), cancellationToken))
                                      .ReturnsAsync(roleItem);

            var roleResponse = new RoleResponse
            {
                Id = roleItem.Id,
                Name = roleItem.Name,
                Properties = new Properties(),
                Permissions = new PermissionResponse()
            };

            //Mock auto mapper
            var mapperMock = new Mock<IMapper>();

            mapperMock.Setup(m => m.Map<RoleCosmosDb>(It.IsAny<RoleModel>()))
                      .Returns(roleItem);
            mapperMock.Setup(m => m.Map<RoleResponse>(It.IsAny<RoleCosmosDb>()))
                     .Returns(roleResponse);

            // System Under Test
            var sut = new RoleSvc(mapperMock.Object, roleCosmosRepositoryMock.Object, actionCosmosRepositoryMock.Object, resourceCosmosRepositoryMock.Object);

            // Act
            var result = await sut.CreateRoleAsync(_roleModel, cancellationToken);

            // Assert

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Id);

            roleCosmosRepositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<RoleCosmosDb>(), cancellationToken), Times.Once);
            mapperMock.Verify(mapper => mapper.Map<RoleCosmosDb>(It.IsAny<RoleModel>()), Times.Once);
            mapperMock.Verify(mapper => mapper.Map<RoleResponse>(It.IsAny<RoleCosmosDb>()), Times.Once);
        }

        [TestMethod]
        public async Task CreateRoleAsync_ThrowsExceptionOnRepositoryFailure()
        {
            // Arrange
            //Create input properties
            var cancellationToken = CancellationToken.None;

            //Mock cosmos repository
            var roleCosmosRepositoryMock = new Mock<IRepository<RoleCosmosDb>>();
            var actionCosmosRepositoryMock = new Mock<IRepository<RbacActionCosmosDb>>();
            var resourceCosmosRepositoryMock = new Mock<IRepository<RbacResourceCosmosDb>>();
            roleCosmosRepositoryMock.Setup(repo => repo.ExistsAsync(It.IsAny<Expression<Func<RoleCosmosDb, bool>>>(), cancellationToken))
                                      .ReturnsAsync(false);

            roleCosmosRepositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<RoleCosmosDb>(), cancellationToken))
                                      .ThrowsAsync(new RBACException());

            //Mock auto mapper
            var _mapperMock = new Mock<IMapper>();

            var roleItem = new RoleCosmosDb
            {
                Name = _roleModel.Name,
                Properties = new RoleProperties(),
                Permissions = new RolePermision(),
            };

            _mapperMock.Setup(m => m.Map<RoleCosmosDb>(It.IsAny<RoleModel>()))
                     .Returns(roleItem);

            // System Under Test
            var sut = new RoleSvc(_mapperMock.Object, roleCosmosRepositoryMock.Object, actionCosmosRepositoryMock.Object, resourceCosmosRepositoryMock.Object);

            // Act and Assert
            await Assert.ThrowsExceptionAsync<RBACException>(() => sut.CreateRoleAsync(_roleModel, cancellationToken));

            Assert.IsNotNull(roleItem.Id);

            roleCosmosRepositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<RoleCosmosDb>(), cancellationToken), Times.Once);
            _mapperMock.Verify(mapper => mapper.Map<RoleCosmosDb>(It.IsAny<RoleModel>()), Times.Once);
            _mapperMock.Verify(mapper => mapper.Map<RoleResponse>(It.IsAny<RoleCosmosDb>()), Times.Never);
        }
    }
}
