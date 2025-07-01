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
    public class GetAllRolesAsyncTests
    {
        [TestMethod]
        public async Task GetAllRolesAsync_ReturnsMappedRolesList()
        {
            // Arrange
            //Create input properties
            var cancellationToken = CancellationToken.None;

            var roleItems = new List<RoleCosmosDb>
            {
                new RoleCosmosDb {
                    Name = "testName",
                    Properties = new RoleProperties
                    {
                        DisplayName = "Name",
                        Description = "test description",
                        Type = "test"
                    },
                    Permissions = new RolePermision()
                },
                new RoleCosmosDb {
                    Name = "testName1",
                    Properties = new RoleProperties
                    {
                        DisplayName = "Name1",
                        Description = "test description1",
                        Type = "test1"
                    },
                    Permissions = new RolePermision()
                }
            };

            //Mock cosmos reposytory
            var roleCosmosRepositoryMock = new Mock<IRepository<RoleCosmosDb>>();
            var actionCosmosRepositoryMock = new Mock<IRepository<RbacActionCosmosDb>>();
            var resourceCosmosRepositoryMock = new Mock<IRepository<RbacResourceCosmosDb>>();
            roleCosmosRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<RoleCosmosDb, bool>>>(), cancellationToken))
                                      .ReturnsAsync(roleItems);

            //Mock auto mapper
            var mapperMock = new Mock<IMapper>();

            var mapperResul = new List<RoleResponse> {
                          new RoleResponse {
                                Name = "testName",
                                Properties = new Properties
                                {
                                    DisplayName = "Name",
                                    Description = "test description",
                                    Type = "test"
                                },
                                Permissions = new PermissionResponse()
                          },
                          new RoleResponse {
                                Name = "testName1",
                                Properties = new Properties
                                {
                                    DisplayName = "Name1",
                                    Description = "test description1",
                                    Type = "test1"
                                },
                                Permissions = new PermissionResponse()
                          }
                      };

            mapperMock.Setup(m => m.Map<List<RoleResponse>>(It.IsAny<List<RoleCosmosDb>>()))
                      .Returns(mapperResul);

            // System Under Test
            var sut = new RoleSvc(mapperMock.Object, roleCosmosRepositoryMock.Object, actionCosmosRepositoryMock.Object, resourceCosmosRepositoryMock.Object);

            // Act
            var result = await sut.GetAllRolesAsync(cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(List<RoleResponse>));

            roleCosmosRepositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<RoleCosmosDb, bool>>>(), cancellationToken), Times.Once);
            mapperMock.Verify(m => m.Map<List<RoleResponse>>(It.IsAny<List<RoleCosmosDb>>()), Times.Once);
        }

        [TestMethod]
        public async Task GetAllRolesAsync_ThrowsExceptionOnRepositoryFailure()
        {
            // Arrange
            //Create input properties
            var cancellationToken = CancellationToken.None;

            //Mock cosmos reposytory
            var roleCosmosRepositoryMock = new Mock<IRepository<RoleCosmosDb>>();
            var actionCosmosRepositoryMock = new Mock<IRepository<RbacActionCosmosDb>>();
            var resourceCosmosRepositoryMock = new Mock<IRepository<RbacResourceCosmosDb>>();
            roleCosmosRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<RoleCosmosDb, bool>>>(), cancellationToken))
                                      .ThrowsAsync(new RBACException("Repository error."));

            //Mock auto mapper
            var mapperMock = new Mock<IMapper>();
            // System Under Test
            var sut = new RoleSvc(mapperMock.Object, roleCosmosRepositoryMock.Object, actionCosmosRepositoryMock.Object, resourceCosmosRepositoryMock.Object);

            // Act and Assert
            await Assert.ThrowsExceptionAsync<RBACException>(() => sut.GetAllRolesAsync(cancellationToken));

            roleCosmosRepositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<RoleCosmosDb, bool>>>(), cancellationToken), Times.Once);
            mapperMock.Verify(m => m.Map<List<RoleResponse>>(It.IsAny<List<RoleCosmosDb>>()), Times.Never);
        }
    }
}
