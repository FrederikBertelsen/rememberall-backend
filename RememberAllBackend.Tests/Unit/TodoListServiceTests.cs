using FluentAssertions;
using Moq;
using RememberAll.src.Entities;
using RememberAll.src.Exceptions;
using RememberAll.src.Repositories.Interfaces;
using RememberAll.src.Services;
using RememberAll.src.Services.Interfaces;
using RememberAll.src.DTOs.Update;
using RememberAllBackend.Tests.Helpers;

namespace RememberAllBackend.Tests.Unit;

public class TodoListServiceTests
{
    [Fact]
    public async Task CreateTodoListAsync_ReturnsDto_WhenValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = TestData.User()
            .WithId(userId)
            .WithName("Bob")
            .WithEmail("bob@example.com")
            .Build();

        var createdList = TestData.TodoList()
            .WithOwnerId(userId)
            .WithName("My List")
            .Build();

        var userRepo = new Mock<IUserRepository>();
        var todoListRepo = new Mock<ITodoListRepository>();
        var listAccessRepo = new Mock<IListAccessRepository>();
        var currentUser = new Mock<ICurrentUserService>();

        currentUser.Setup(c => c.GetUserId()).Returns(userId);
        userRepo.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync(user);
        todoListRepo.Setup(r => r.CreateTodoListAsync(It.IsAny<TodoList>())).ReturnsAsync(createdList);

        var service = new TodoListService(userRepo.Object, todoListRepo.Object, listAccessRepo.Object, currentUser.Object);

        // Act
        var dto = await service.CreateTodoListAsync(TestData.CreateTodoListDto("My List"));

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(createdList.Id);
        dto.Name.Should().Be("My List");
        todoListRepo.Verify(r => r.CreateTodoListAsync(It.IsAny<TodoList>()), Times.Once);
        todoListRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetTodoListByIdAsync_ThrowsMissingValue_WhenEmptyGuid()
    {
        // Arrange
        var userRepo = new Mock<IUserRepository>();
        var todoListRepo = new Mock<ITodoListRepository>();
        var listAccessRepo = new Mock<IListAccessRepository>();
        var currentUser = new Mock<ICurrentUserService>();

        var service = new TodoListService(userRepo.Object, todoListRepo.Object, listAccessRepo.Object, currentUser.Object);

        // Act & Assert
        await Assert.ThrowsAsync<MissingValueException>(() => service.GetTodoListByIdAsync(Guid.Empty));
    }

    [Fact]
    public async Task UpdateTodoListAsync_ThrowsForbidden_WhenNotOwner()
    {
        // Arrange
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var listId = Guid.NewGuid();

        var existingList = TestData.TodoList()
            .WithOwnerId(ownerId)
            .WithId(listId)
            .WithName("Owner's List")
            .Build();

        var userRepo = new Mock<IUserRepository>();
        var todoListRepo = new Mock<ITodoListRepository>();
        var listAccessRepo = new Mock<IListAccessRepository>();
        var currentUser = new Mock<ICurrentUserService>();

        currentUser.Setup(c => c.GetUserId()).Returns(otherUserId);
        todoListRepo.Setup(r => r.GetTodoListByIdAsync(listId)).ReturnsAsync(existingList);

        var service = new TodoListService(userRepo.Object, todoListRepo.Object, listAccessRepo.Object, currentUser.Object);
        var updateDto = new UpdateTodoListDto(listId, "Hacked Name");

        // Act & Assert
        await Assert.ThrowsAsync<AuthException>(
            () => service.UpdateTodoListAsync(updateDto));

        todoListRepo.Verify(r => r.UpdateTodoList(It.IsAny<TodoList>()), Times.Never);
        todoListRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }
}

