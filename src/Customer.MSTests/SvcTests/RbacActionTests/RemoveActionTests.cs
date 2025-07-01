using AutoMapper;
using CommonModels.Enum;
using CustomerCustomerApi.Exceptions;
using CustomerCustomerApi.Models;
using CustomerCustomerApi.Services;
using Microsoft.Azure.CosmosRepository;
using Moq;
using System.Linq.Expressions;

namespace CustomerCustomer.MSTests.SvcTests.RbacActionTests
{
    [TestClass]
    public class RemoveActionTests
    {
        [TestMethod]
        public async Task RemoveActionsAsync_RemoveActionAndReturnsResponse()
        {
            // Arrange
            //Create input properties
            var cancellationToken = CancellationToken.None;
            var testId = "action-123";
            var createdActionItem = new List<RbacActionCosmosDb>() {
                new RbacActionCosmosDb
                {
                    Id = testId,
                    Category = ActionCategory.dataAction.ToString(),
                    Action = "create",
                    Description = "some description"
                }
            };

            //Mock cosmos repository
            var actionCosmosRepositoryMock = new Mock<IRepository<RbacActionCosmosDb>>();

            actionCosmosRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<RbacActionCosmosDb, bool>>>(), cancellationToken))
                .Returns(ValueTask.FromResult<IEnumerable<RbacActionCosmosDb>>(createdActionItem));
            //Mock auto mapper
            var mapperMock = new Mock<IMapper>();

            // System Under Test
            var sut = new RbacActionSvc(mapperMock.Object, actionCosmosRepositoryMock.Object);

            // Act
            await sut.RemoveActionAsync(createdActionItem.First().Id, cancellationToken);

            // Assert
            actionCosmosRepositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<RbacActionCosmosDb, bool>>>(), cancellationToken), Times.Once);
            actionCosmosRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<RbacActionCosmosDb>(a => a.Id == testId && a.IsDeleted), false, cancellationToken), Times.Once);
        }

        [TestMethod]
        public async Task RemoveActionAsync_ThrowsExceptionOnRepositoryFailure()
        {
            // Arrange
            //Create input properties
            var cancellationToken = CancellationToken.None;
            var testId = "action-456";

            //Mock cosmos repository
            var actionCosmosRepositoryMock = new Mock<IRepository<RbacActionCosmosDb>>();

            var createdActionItem = new List<RbacActionCosmosDb>() {
                new RbacActionCosmosDb
                {
                    Id = testId,
                    Category = ActionCategory.dataAction.ToString(),
                    Action = "create",
                    Description = "some description"
                }
            };

            actionCosmosRepositoryMock.Setup(repo => repo.GetAsync(It.IsAny<Expression<Func<RbacActionCosmosDb, bool>>>(), cancellationToken))
                                    .Returns(ValueTask.FromResult<IEnumerable<RbacActionCosmosDb>>(createdActionItem));

            actionCosmosRepositoryMock.Setup(repo => repo.UpdateAsync(createdActionItem.First(), false, cancellationToken))
                                      .ThrowsAsync(new Exception("Repository error."));

            //Mock auto mapper
            var mapperMock = new Mock<IMapper>();

            // System Under Test
            var sut = new RbacActionSvc(mapperMock.Object, actionCosmosRepositoryMock.Object);

            // Act and Assert
            await Assert.ThrowsExceptionAsync<RBACException>(() => sut.RemoveActionAsync(It.IsAny<string>(), cancellationToken));

            actionCosmosRepositoryMock.Verify(repo => repo.GetAsync(It.IsAny<Expression<Func<RbacActionCosmosDb, bool>>>(), cancellationToken), Times.Once);
            actionCosmosRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<RbacActionCosmosDb>(a => a.Id == testId && a.IsDeleted), false, cancellationToken), Times.Once);
        }
    }
}