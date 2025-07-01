using AutoMapper;
using CommonModels.Enum;
using CustomerCustomerApi.Exceptions;
using CustomerCustomerApi.Models;
using CustomerCustomerApi.Models.Rbac;
using CustomerCustomerApi.Services;
using Microsoft.Azure.CosmosRepository;
using Moq;
using System.Linq.Expressions;

namespace CustomerCustomer.MSTests.SvcTests.RbacActionTests
{
    [TestClass]
    public class GetActionByCategoryTests
    {
        [TestMethod]
        public async Task GetActionsByCategoryAsync_ReturnsMappedActionList()
        {
            // Arrange
            //Create input properties
            var cancellationToken = CancellationToken.None;
            var category = ActionCategory.uiAction;

            var actionItems = new List<RbacActionCosmosDb>
            {
                new RbacActionCosmosDb {
                    Category = ActionCategory.uiAction.ToString(),
                    Description = "test uiAction description",
                    Action = "read",
                    Id = "123-123-123-123"
                },
               new RbacActionCosmosDb {
                    Category = ActionCategory.dataAction.ToString(),
                    Description = "test dataAction description",
                    Action = "all",
                    Id = "111-1111-111-1111"
                }
            };

            //Mock cosmos repository
            var actionCosmosRepositoryMock = new Mock<IRepository<RbacActionCosmosDb>>();
            actionCosmosRepositoryMock.Setup(repo => repo.GetByQueryAsync(It.IsAny<string>(), cancellationToken))
                                      .ReturnsAsync(actionItems);

            //Mock auto mapper
            var mapperMock = new Mock<IMapper>();

            var mapperResul = new List<RbacActionResponse> {
                          new RbacActionResponse
                          {
                            Category = ActionCategory.uiAction.ToString(),
                            Description = "test uiAction description",
                            Action = "read",
                            Id = "123-123-123-123"
                          },
                          new RbacActionResponse
                          {
                            Category = ActionCategory.dataAction.ToString(),
                            Description = "test dataAction description",
                            Action = "all",
                            Id = "111-1111-111-1111"
                          }
                      };

            mapperMock.Setup(m => m.Map<List<RbacActionResponse>>(It.IsAny<List<RbacActionCosmosDb>>()))
                      .Returns(mapperResul);

            // System Under Test
            var sut = new RbacActionSvc(mapperMock.Object, actionCosmosRepositoryMock.Object);

            // Act
            var result = await sut.GetActionsByCategoryAsync(category, cancellationToken);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(List<RbacActionResponse>));

            actionCosmosRepositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<RbacActionCosmosDb, bool>>>(), cancellationToken), Times.Once);
            mapperMock.Verify(m => m.Map<List<RbacActionResponse>>(It.IsAny<List<RbacActionCosmosDb>>()), Times.Once);
        }

        [TestMethod]
        public async Task GetActionsByCategoryAsync_ThrowsExceptionOnRepositoryFailure()
        {
            // Arrange
            //Create input properties
            var cancellationToken = CancellationToken.None;
            var category = ActionCategory.uiAction;

            //Mock cosmos repository
            var actionCosmosRepositoryMock = new Mock<IRepository<RbacActionCosmosDb>>();
            actionCosmosRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<RbacActionCosmosDb, bool>>>(), cancellationToken))
                                      .ThrowsAsync(new Exception("Repository error."));

            //Mock auto mapper
            var mapperMock = new Mock<IMapper>();
            // System Under Test
            var sut = new RbacActionSvc(mapperMock.Object, actionCosmosRepositoryMock.Object);

            // Act and Assert
            await Assert.ThrowsExceptionAsync<RBACException>(() => sut.GetActionsByCategoryAsync(category, cancellationToken));

            actionCosmosRepositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<RbacActionCosmosDb, bool>>>(), cancellationToken), Times.Once);
            mapperMock.Verify(m => m.Map<List<RbacActionResponse>>(It.IsAny<List<RbacActionCosmosDb>>()), Times.Never);
        }
    }
}
