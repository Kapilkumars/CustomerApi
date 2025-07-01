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
    public class CreateRbacActionTests
    {
        [TestMethod]
        public async Task CreateActionAsync_CreatesActionAndReturnsResponse()
        {
            // Arrange
            //Create input properties
            var cancellationToken = CancellationToken.None;
            var actionModel = new RbacActionModel
            {
                Category = ActionCategory.dataAction,
                Action = "create",
                Description = "some description"
            };

            //Mock cosmos repository
            var actionCosmosRepositoryMock = new Mock<IRepository<RbacActionCosmosDb>>();
            actionCosmosRepositoryMock.Setup(repo => repo.ExistsAsync(It.IsAny<Expression<Func<RbacActionCosmosDb, bool>>>(), cancellationToken))
                                      .ReturnsAsync(false);

            var createdActionItem = new RbacActionCosmosDb
            {
                Category = actionModel.Category.ToString(),
                Action = actionModel.Action,
                Description = actionModel.Description
            };

            actionCosmosRepositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<RbacActionCosmosDb>(), cancellationToken))
                                      .ReturnsAsync(createdActionItem);

            var createdActionResponse = new RbacActionResponse
            {
                Id = createdActionItem.Id,
                Category = createdActionItem.Category,
                Action = createdActionItem.Action,
                Description = createdActionItem.Description
            };

            //Mock auto mapper
            var mapperMock = new Mock<IMapper>();

            mapperMock.Setup(m => m.Map<RbacActionCosmosDb>(It.IsAny<RbacActionModel>()))
                      .Returns(createdActionItem);
            mapperMock.Setup(m => m.Map<RbacActionResponse>(It.IsAny<RbacActionCosmosDb>()))
                     .Returns(createdActionResponse);

            // System Under Test
            var sut = new RbacActionSvc(mapperMock.Object, actionCosmosRepositoryMock.Object);

            // Act
            var result = await sut.CreateActionAsync(actionModel, cancellationToken);

            // Assert
            Assert.IsNotNull(result.Id);
            Assert.IsNotNull(result);

            actionCosmosRepositoryMock.Verify(repo => repo.ExistsAsync(It.IsAny<Expression<Func<RbacActionCosmosDb, bool>>>(), cancellationToken), Times.Once);
            actionCosmosRepositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<RbacActionCosmosDb>(), cancellationToken), Times.Once);
            mapperMock.Verify(mapper => mapper.Map<RbacActionCosmosDb>(It.IsAny<RbacActionModel>()), Times.Once);
            mapperMock.Verify(mapper => mapper.Map<RbacActionResponse>(It.IsAny<RbacActionCosmosDb>()), Times.Once);
        }

        [TestMethod]
        public async Task CreateActionAsync_WithExistingAction_ThrowsInvalidDataException()
        {
            // Arrange
            //Create input properties
            var cancellationToken = CancellationToken.None;
            var actionModel = new RbacActionModel
            {
                Category = ActionCategory.dataAction,
                Action = "create",
                Description = "some description"
            };

            //Mock cosmos repository
            var actionCosmosRepositoryMock = new Mock<IRepository<RbacActionCosmosDb>>();
            actionCosmosRepositoryMock.Setup(repo => repo.ExistsAsync(It.IsAny<Expression<Func<RbacActionCosmosDb, bool>>>(), cancellationToken))
                                      .ReturnsAsync(true);

            //Mock auto mapper
            var mapperMock = new Mock<IMapper>();
            // System Under Test
            var sut = new RbacActionSvc(mapperMock.Object, actionCosmosRepositoryMock.Object);

            // Act and Assert
            await Assert.ThrowsExceptionAsync<RBACException>(() => sut.CreateActionAsync(actionModel, cancellationToken));

            actionCosmosRepositoryMock.Verify(repo => repo.ExistsAsync(It.IsAny<Expression<Func<RbacActionCosmosDb, bool>>>(), cancellationToken), Times.Once);
            actionCosmosRepositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<RbacActionCosmosDb>(), cancellationToken), Times.Never);
            mapperMock.Verify(mapper => mapper.Map<RbacActionCosmosDb>(It.IsAny<RbacActionModel>()), Times.Never);
            mapperMock.Verify(mapper => mapper.Map<RbacActionResponse>(It.IsAny<RbacActionCosmosDb>()), Times.Never);
        }


        [TestMethod]
        public async Task CreateActionAsync_ThrowsExceptionOnRepositoryFailure()
        {
            // Arrange
            //Create input properties
            var cancellationToken = CancellationToken.None;
            var actionModel = new RbacActionModel
            {
                Category = ActionCategory.dataAction,
                Action = "create",
                Description = "some description"
            };

            //Mock cosmos repository
            var actionCosmosRepositoryMock = new Mock<IRepository<RbacActionCosmosDb>>();
            actionCosmosRepositoryMock.Setup(repo => repo.ExistsAsync(It.IsAny<Expression<Func<RbacActionCosmosDb, bool>>>(), cancellationToken))
                                      .ReturnsAsync(false);

            actionCosmosRepositoryMock.Setup(repo => repo.CreateAsync(It.IsAny<RbacActionCosmosDb>(), cancellationToken))
                                      .ThrowsAsync(new Exception());

            //Mock auto mapper
            var mapperMock = new Mock<IMapper>();

            var createdActionItem = new RbacActionCosmosDb
            {
                Category = actionModel.Category.ToString(),
                Action = actionModel.Action,
                Description = actionModel.Description
            };

            mapperMock.Setup(m => m.Map<RbacActionCosmosDb>(It.IsAny<RbacActionModel>()))
                     .Returns(createdActionItem);

            // System Under Test
            var sut = new RbacActionSvc(mapperMock.Object, actionCosmosRepositoryMock.Object);

            // Act and Assert
            await Assert.ThrowsExceptionAsync<RBACException>(() => sut.CreateActionAsync(actionModel, cancellationToken));

            Assert.IsNotNull(createdActionItem.Id);

            actionCosmosRepositoryMock.Verify(repo => repo.ExistsAsync(It.IsAny<Expression<Func<RbacActionCosmosDb, bool>>>(), cancellationToken), Times.Once);
            actionCosmosRepositoryMock.Verify(repo => repo.CreateAsync(It.IsAny<RbacActionCosmosDb>(), cancellationToken), Times.Once);
            mapperMock.Verify(mapper => mapper.Map<RbacActionCosmosDb>(It.IsAny<RbacActionModel>()), Times.Once);
            mapperMock.Verify(mapper => mapper.Map<RbacActionResponse>(It.IsAny<RbacActionCosmosDb>()), Times.Never);
        }
    }
}
