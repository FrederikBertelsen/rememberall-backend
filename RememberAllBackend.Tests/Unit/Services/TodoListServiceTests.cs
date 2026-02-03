using FluentAssertions;
using Moq;
using RememberAll.src.Entities;
using RememberAll.src.Exceptions;
using RememberAll.src.Repositories.Interfaces;
using RememberAll.src.Services;
using RememberAll.src.Services.Interfaces;
using RememberAll.src.DTOs.Update;
using RememberAllBackend.Tests.Helpers;

namespace RememberAllBackend.Tests.Unit.Services;

public class TodoListServiceTests
{
    #region CreateTodoListAsync Tests

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
    public async Task CreateTodoListAsync_ThrowsNotFound_WhenUserNotExists()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var userRepo = new Mock<IUserRepository>();
        var todoListRepo = new Mock<ITodoListRepository>();
        var listAccessRepo = new Mock<IListAccessRepository>();
        var currentUser = new Mock<ICurrentUserService>();

        currentUser.Setup(c => c.GetUserId()).Returns(userId);
        userRepo.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync((User?)null);

        var service = new TodoListService(userRepo.Object, todoListRepo.Object, listAccessRepo.Object, currentUser.Object);

        // Act & Assert
        await service.Invoking(s => s.CreateTodoListAsync(TestData.CreateTodoListDto("My List")))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateTodoListAsync_ThrowsMissingValue_WhenNameInvalid(string invalidName)
    {
        // Arrange
        var userRepo = new Mock<IUserRepository>();
        var todoListRepo = new Mock<ITodoListRepository>();
        var listAccessRepo = new Mock<IListAccessRepository>();
        var currentUser = new Mock<ICurrentUserService>();

        var service = new TodoListService(userRepo.Object, todoListRepo.Object, listAccessRepo.Object, currentUser.Object);

        // Act & Assert
        await service.Invoking(s => s.CreateTodoListAsync(TestData.CreateTodoListDto(invalidName)))
            .Should().ThrowAsync<MissingValueException>();
    }

    #endregion

    #region GetTodoListByIdAsync Tests

    [Fact]
    public async Task GetTodoListByIdAsync_ReturnsDto_WhenValidAndHasAccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var listId = Guid.NewGuid();

        var todoList = TestData.TodoList()
            .WithId(listId)
            .WithOwnerId(userId)
            .WithName("My List")
            .Build();

        var userRepo = new Mock<IUserRepository>();
        var todoListRepo = new Mock<ITodoListRepository>();
        var listAccessRepo = new Mock<IListAccessRepository>();
        var currentUser = new Mock<ICurrentUserService>();

        currentUser.Setup(c => c.GetUserId()).Returns(userId);
        todoListRepo.Setup(r => r.GetTodoListByIdAsync(listId)).ReturnsAsync(todoList);
        listAccessRepo.Setup(r => r.UserHasAccessToListAsync(userId, listId)).ReturnsAsync(true);

        var service = new TodoListService(userRepo.Object, todoListRepo.Object, listAccessRepo.Object, currentUser.Object);

        // Act
        var result = await service.GetTodoListByIdAsync(listId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(listId);
        result.Name.Should().Be("My List");
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
    public async Task GetTodoListByIdAsync_ThrowsNotFound_WhenListNotExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var listId = Guid.NewGuid();

        var userRepo = new Mock<IUserRepository>();
        var todoListRepo = new Mock<ITodoListRepository>();
        var listAccessRepo = new Mock<IListAccessRepository>();
        var currentUser = new Mock<ICurrentUserService>();

        currentUser.Setup(c => c.GetUserId()).Returns(userId);
        todoListRepo.Setup(r => r.GetTodoListByIdAsync(listId)).ReturnsAsync((TodoList?)null);

        var service = new TodoListService(userRepo.Object, todoListRepo.Object, listAccessRepo.Object, currentUser.Object);

        // Act & Assert
        await service.Invoking(s => s.GetTodoListByIdAsync(listId))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetTodoListByIdAsync_ThrowsAuthException_WhenUserNoAccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var listId = Guid.NewGuid();

        var todoList = TestData.TodoList()
            .WithId(listId)
            .WithOwnerId(Guid.NewGuid())
            .WithName("Someone Else's List")
            .Build();

        var userRepo = new Mock<IUserRepository>();
        var todoListRepo = new Mock<ITodoListRepository>();
        var listAccessRepo = new Mock<IListAccessRepository>();
        var currentUser = new Mock<ICurrentUserService>();

        currentUser.Setup(c => c.GetUserId()).Returns(userId);
        todoListRepo.Setup(r => r.GetTodoListByIdAsync(listId)).ReturnsAsync(todoList);
        listAccessRepo.Setup(r => r.UserHasAccessToListAsync(userId, listId)).ReturnsAsync(false);

        var service = new TodoListService(userRepo.Object, todoListRepo.Object, listAccessRepo.Object, currentUser.Object);

        // Act & Assert
        await service.Invoking(s => s.GetTodoListByIdAsync(listId))
            .Should().ThrowAsync<ForbiddenException>();
    }

    #endregion

    #region GetTodoListsByUserIdAsync Tests

    [Fact]
    public async Task GetTodoListsByUserIdAsync_ReturnsLists_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var todoLists = new List<TodoList>
        {
            TestData.TodoList().WithOwnerId(userId).WithName("List 1").Build(),
            TestData.TodoList().WithOwnerId(userId).WithName("List 2").Build()
        };

        var userRepo = new Mock<IUserRepository>();
        var todoListRepo = new Mock<ITodoListRepository>();
        var listAccessRepo = new Mock<IListAccessRepository>();
        var currentUser = new Mock<ICurrentUserService>();

        currentUser.Setup(c => c.GetUserId()).Returns(userId);
        userRepo.Setup(r => r.UserExistsByIdAsync(userId)).ReturnsAsync(true);
        todoListRepo.Setup(r => r.GetTodoListsByUserIdAsync(userId)).ReturnsAsync(todoLists);

        var service = new TodoListService(userRepo.Object, todoListRepo.Object, listAccessRepo.Object, currentUser.Object);

        // Act
        var result = await service.GetTodoListsByUserIdAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().ContainSingle(t => t.Name == "List 1");
        result.Should().ContainSingle(t => t.Name == "List 2");
    }

    [Fact]
    public async Task GetTodoListsByUserIdAsync_ThrowsNotFound_WhenUserNotExists()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var userRepo = new Mock<IUserRepository>();
        var todoListRepo = new Mock<ITodoListRepository>();
        var listAccessRepo = new Mock<IListAccessRepository>();
        var currentUser = new Mock<ICurrentUserService>();

        currentUser.Setup(c => c.GetUserId()).Returns(userId);
        userRepo.Setup(r => r.UserExistsByIdAsync(userId)).ReturnsAsync(false);

        var service = new TodoListService(userRepo.Object, todoListRepo.Object, listAccessRepo.Object, currentUser.Object);

        // Act & Assert
        await service.Invoking(s => s.GetTodoListsByUserIdAsync())
            .Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region UpdateTodoListAsync Tests

    [Fact]
    public async Task UpdateTodoListAsync_ReturnsDto_WhenValidAndHasAccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var listId = Guid.NewGuid();

        var existingList = TestData.TodoList()
            .WithOwnerId(userId)
            .WithId(listId)
            .WithName("Original Name")
            .Build();

        var userRepo = new Mock<IUserRepository>();
        var todoListRepo = new Mock<ITodoListRepository>();
        var listAccessRepo = new Mock<IListAccessRepository>();
        var currentUser = new Mock<ICurrentUserService>();

        currentUser.Setup(c => c.GetUserId()).Returns(userId);
        todoListRepo.Setup(r => r.GetTodoListByIdAsync(listId)).ReturnsAsync(existingList);
        listAccessRepo.Setup(r => r.UserHasAccessToListAsync(userId, listId)).ReturnsAsync(true);

        var service = new TodoListService(userRepo.Object, todoListRepo.Object, listAccessRepo.Object, currentUser.Object);
        var updateDto = new UpdateTodoListDto(listId, "Updated Name");

        // Act
        var result = await service.UpdateTodoListAsync(updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(listId);
        result.Name.Should().Be("Updated Name");
        todoListRepo.Verify(r => r.UpdateTodoList(It.IsAny<TodoList>()), Times.Once);
        todoListRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateTodoListAsync_ThrowsNotFound_WhenListNotExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var listId = Guid.NewGuid();

        var userRepo = new Mock<IUserRepository>();
        var todoListRepo = new Mock<ITodoListRepository>();
        var listAccessRepo = new Mock<IListAccessRepository>();
        var currentUser = new Mock<ICurrentUserService>();

        todoListRepo.Setup(r => r.GetTodoListByIdAsync(listId)).ReturnsAsync((TodoList?)null);

        var service = new TodoListService(userRepo.Object, todoListRepo.Object, listAccessRepo.Object, currentUser.Object);
        var updateDto = new UpdateTodoListDto(listId, "Updated Name");

        // Act & Assert
        await service.Invoking(s => s.UpdateTodoListAsync(updateDto))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateTodoListAsync_ThrowsAuthException_WhenUserNoAccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var listId = Guid.NewGuid();

        var existingList = TestData.TodoList()
            .WithOwnerId(otherUserId)
            .WithId(listId)
            .WithName("Other's List")
            .Build();

        var userRepo = new Mock<IUserRepository>();
        var todoListRepo = new Mock<ITodoListRepository>();
        var listAccessRepo = new Mock<IListAccessRepository>();
        var currentUser = new Mock<ICurrentUserService>();

        currentUser.Setup(c => c.GetUserId()).Returns(userId);
        todoListRepo.Setup(r => r.GetTodoListByIdAsync(listId)).ReturnsAsync(existingList);
        listAccessRepo.Setup(r => r.UserHasAccessToListAsync(userId, listId)).ReturnsAsync(false);

        var service = new TodoListService(userRepo.Object, todoListRepo.Object, listAccessRepo.Object, currentUser.Object);
        var updateDto = new UpdateTodoListDto(listId, "Hacked Name");

        // Act & Assert
        await service.Invoking(s => s.UpdateTodoListAsync(updateDto))
            .Should().ThrowAsync<ForbiddenException>();

        todoListRepo.Verify(r => r.UpdateTodoList(It.IsAny<TodoList>()), Times.Never);
        todoListRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    #endregion

    #region DeleteTodoList Tests

    [Fact]
    public async Task DeleteTodoList_DeletesSuccessfully_WhenOwnerAndValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var listId = Guid.NewGuid();

        var todoList = TestData.TodoList()
            .WithOwnerId(userId)
            .WithId(listId)
            .WithName("My List")
            .Build();

        var userRepo = new Mock<IUserRepository>();
        var todoListRepo = new Mock<ITodoListRepository>();
        var listAccessRepo = new Mock<IListAccessRepository>();
        var currentUser = new Mock<ICurrentUserService>();

        currentUser.Setup(c => c.GetUserId()).Returns(userId);
        todoListRepo.Setup(r => r.GetTodoListByIdAsync(listId)).ReturnsAsync(todoList);

        var service = new TodoListService(userRepo.Object, todoListRepo.Object, listAccessRepo.Object, currentUser.Object);

        // Act
        await service.DeleteTodoListAsync(listId);

        // Assert
        todoListRepo.Verify(r => r.DeleteTodoList(todoList), Times.Once);
        todoListRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteTodoList_ThrowsMissingValue_WhenEmptyGuid()
    {
        // Arrange
        var userRepo = new Mock<IUserRepository>();
        var todoListRepo = new Mock<ITodoListRepository>();
        var listAccessRepo = new Mock<IListAccessRepository>();
        var currentUser = new Mock<ICurrentUserService>();

        var service = new TodoListService(userRepo.Object, todoListRepo.Object, listAccessRepo.Object, currentUser.Object);

        // Act & Assert
        await service.Invoking(s => s.DeleteTodoListAsync(Guid.Empty))
            .Should().ThrowAsync<MissingValueException>();
    }

    [Fact]
    public async Task DeleteTodoList_ThrowsNotFound_WhenListNotExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var listId = Guid.NewGuid();

        var userRepo = new Mock<IUserRepository>();
        var todoListRepo = new Mock<ITodoListRepository>();
        var listAccessRepo = new Mock<IListAccessRepository>();
        var currentUser = new Mock<ICurrentUserService>();

        currentUser.Setup(c => c.GetUserId()).Returns(userId);
        todoListRepo.Setup(r => r.GetTodoListByIdAsync(listId)).ReturnsAsync((TodoList?)null);

        var service = new TodoListService(userRepo.Object, todoListRepo.Object, listAccessRepo.Object, currentUser.Object);

        // Act & Assert
        await service.Invoking(s => s.DeleteTodoListAsync(listId))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteTodoList_ThrowsAuthException_WhenNotOwner()
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

        // Act & Assert
        await service.Invoking(s => s.DeleteTodoListAsync(listId))
            .Should().ThrowAsync<ForbiddenException>();

        todoListRepo.Verify(r => r.DeleteTodoList(It.IsAny<TodoList>()), Times.Never);
        todoListRepo.Verify(r => r.SaveChangesAsync(), Times.Never);
    }

    #endregion
}

