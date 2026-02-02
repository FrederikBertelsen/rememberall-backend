using FluentAssertions;
using Moq;
using RememberAll.src.DTOs.Update;
using RememberAll.src.Entities;
using RememberAll.src.Exceptions;
using RememberAll.src.Repositories.Interfaces;
using RememberAll.src.Services;
using RememberAll.src.Services.Interfaces;
using RememberAllBackend.Tests.Helpers;

namespace RememberAllBackend.Tests.Unit.Services;

public class TodoItemServiceTests
{
    #region CreateTodoItemAsync Tests

    [Fact]
    public async Task CreateTodoItemAsync_ReturnsTodoItemDto_WhenValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var list = TestData.TodoList().WithOwnerId(userId).Build();
        var item = TestData.TodoItem().WithTodoListId(list.Id).WithText("Buy milk").Build();

        var mockTodoListRepo = new Mock<ITodoListRepository>();
        var mockTodoItemRepo = new Mock<ITodoItemRepository>();
        var mockListAccessRepo = new Mock<IListAccessRepository>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        mockCurrentUser.Setup(c => c.GetUserId()).Returns(userId);
        mockTodoListRepo.Setup(r => r.GetTodoListByIdAsync(list.Id)).ReturnsAsync(list);
        mockListAccessRepo.Setup(r => r.UserHasAccessToListAsync(userId, list.Id)).ReturnsAsync(true);
        mockTodoItemRepo.Setup(r => r.CreateTodoItemAsync(It.IsAny<TodoItem>())).ReturnsAsync(item);

        var service = new TodoItemService(mockTodoListRepo.Object, mockTodoItemRepo.Object, mockListAccessRepo.Object, mockCurrentUser.Object);
        var dto = TestData.CreateTodoItemDto(list.Id, "Buy milk");

        // Act
        var result = await service.CreateTodoItemAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Text.Should().Be("Buy milk");
        result.Id.Should().NotBe(Guid.Empty);
        mockTodoItemRepo.Verify(r => r.CreateTodoItemAsync(It.IsAny<TodoItem>()), Times.Once);
        mockTodoItemRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateTodoItemAsync_ThrowsMissingValue_WhenTextInvalid(string invalidText)
    {
        // Arrange
        var mockTodoListRepo = new Mock<ITodoListRepository>();
        var mockTodoItemRepo = new Mock<ITodoItemRepository>();
        var mockListAccessRepo = new Mock<IListAccessRepository>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        var service = new TodoItemService(mockTodoListRepo.Object, mockTodoItemRepo.Object, mockListAccessRepo.Object, mockCurrentUser.Object);
        var dto = TestData.CreateTodoItemDto(Guid.NewGuid(), invalidText);

        // Act & Assert
        await service.Invoking(s => s.CreateTodoItemAsync(dto))
            .Should().ThrowAsync<MissingValueException>();
    }

    [Fact]
    public async Task CreateTodoItemAsync_ThrowsNotFound_WhenListNotExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var listId = Guid.NewGuid();

        var mockTodoListRepo = new Mock<ITodoListRepository>();
        var mockTodoItemRepo = new Mock<ITodoItemRepository>();
        var mockListAccessRepo = new Mock<IListAccessRepository>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        mockCurrentUser.Setup(c => c.GetUserId()).Returns(userId);
        mockTodoListRepo.Setup(r => r.GetTodoListByIdAsync(listId)).ReturnsAsync((TodoList?)null);

        var service = new TodoItemService(mockTodoListRepo.Object, mockTodoItemRepo.Object, mockListAccessRepo.Object, mockCurrentUser.Object);
        var dto = TestData.CreateTodoItemDto(listId);

        // Act & Assert
        await service.Invoking(s => s.CreateTodoItemAsync(dto))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateTodoItemAsync_ThrowsAuth_WhenUserHasNoAccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var list = TestData.TodoList().WithOwnerId(userId).Build();

        var mockTodoListRepo = new Mock<ITodoListRepository>();
        var mockTodoItemRepo = new Mock<ITodoItemRepository>();
        var mockListAccessRepo = new Mock<IListAccessRepository>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        mockCurrentUser.Setup(c => c.GetUserId()).Returns(userId);
        mockTodoListRepo.Setup(r => r.GetTodoListByIdAsync(list.Id)).ReturnsAsync(list);
        mockListAccessRepo.Setup(r => r.UserHasAccessToListAsync(userId, list.Id)).ReturnsAsync(false);

        var service = new TodoItemService(mockTodoListRepo.Object, mockTodoItemRepo.Object, mockListAccessRepo.Object, mockCurrentUser.Object);
        var dto = TestData.CreateTodoItemDto(list.Id);

        // Act & Assert
        await service.Invoking(s => s.CreateTodoItemAsync(dto))
            .Should().ThrowAsync<AuthException>();
    }

    #endregion

    #region UpdateTodoItem Tests

    [Fact]
    public async Task UpdateTodoItem_ReturnsTodoItemDto_WhenValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var list = TestData.TodoList().WithOwnerId(userId).Build();
        var item = TestData.TodoItem().WithTodoListId(list.Id).WithText("Old text").Build();

        var mockTodoListRepo = new Mock<ITodoListRepository>();
        var mockTodoItemRepo = new Mock<ITodoItemRepository>();
        var mockListAccessRepo = new Mock<IListAccessRepository>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        mockCurrentUser.Setup(c => c.GetUserId()).Returns(userId);
        mockTodoItemRepo.Setup(r => r.GetTodoItemByIdAsync(item.Id)).ReturnsAsync(item);
        mockListAccessRepo.Setup(r => r.UserHasAccessToListAsync(userId, list.Id)).ReturnsAsync(true);

        var service = new TodoItemService(mockTodoListRepo.Object, mockTodoItemRepo.Object, mockListAccessRepo.Object, mockCurrentUser.Object);
        var dto = new UpdateTodoItemDto(item.Id, "New text");

        // Act
        var result = await service.UpdateTodoItem(dto);

        // Assert
        result.Should().NotBeNull();
        mockTodoItemRepo.Verify(r => r.UpdateTodoItem(It.IsAny<TodoItem>()), Times.Once);
        mockTodoItemRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateTodoItem_ThrowsNotFound_WhenItemNotExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        var mockTodoListRepo = new Mock<ITodoListRepository>();
        var mockTodoItemRepo = new Mock<ITodoItemRepository>();
        var mockListAccessRepo = new Mock<IListAccessRepository>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        mockCurrentUser.Setup(c => c.GetUserId()).Returns(userId);
        mockTodoItemRepo.Setup(r => r.GetTodoItemByIdAsync(itemId)).ReturnsAsync((TodoItem?)null);

        var service = new TodoItemService(mockTodoListRepo.Object, mockTodoItemRepo.Object, mockListAccessRepo.Object, mockCurrentUser.Object);
        var dto = new UpdateTodoItemDto(itemId, "New text");

        // Act & Assert
        await service.Invoking(s => s.UpdateTodoItem(dto))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateTodoItem_ThrowsAuth_WhenUserHasNoAccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var list = TestData.TodoList().WithOwnerId(userId).Build();
        var item = TestData.TodoItem().WithTodoListId(list.Id).Build();

        var mockTodoListRepo = new Mock<ITodoListRepository>();
        var mockTodoItemRepo = new Mock<ITodoItemRepository>();
        var mockListAccessRepo = new Mock<IListAccessRepository>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        mockCurrentUser.Setup(c => c.GetUserId()).Returns(userId);
        mockTodoItemRepo.Setup(r => r.GetTodoItemByIdAsync(item.Id)).ReturnsAsync(item);
        mockListAccessRepo.Setup(r => r.UserHasAccessToListAsync(userId, list.Id)).ReturnsAsync(false);

        var service = new TodoItemService(mockTodoListRepo.Object, mockTodoItemRepo.Object, mockListAccessRepo.Object, mockCurrentUser.Object);
        var dto = new UpdateTodoItemDto(item.Id, "New text");

        // Act & Assert
        await service.Invoking(s => s.UpdateTodoItem(dto))
            .Should().ThrowAsync<AuthException>();
    }

    #endregion

    #region MarkTodoItemAsCompleteAsync Tests

    [Fact]
    public async Task MarkTodoItemAsCompleteAsync_ReturnsTodoItemDto_WhenValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var list = TestData.TodoList().WithOwnerId(userId).Build();
        var item = TestData.TodoItem().WithTodoListId(list.Id).AsIncomplete().Build();

        var mockTodoListRepo = new Mock<ITodoListRepository>();
        var mockTodoItemRepo = new Mock<ITodoItemRepository>();
        var mockListAccessRepo = new Mock<IListAccessRepository>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        mockCurrentUser.Setup(c => c.GetUserId()).Returns(userId);
        mockTodoItemRepo.Setup(r => r.GetTodoItemByIdAsync(item.Id)).ReturnsAsync(item);
        mockListAccessRepo.Setup(r => r.UserHasAccessToListAsync(userId, list.Id)).ReturnsAsync(true);

        var service = new TodoItemService(mockTodoListRepo.Object, mockTodoItemRepo.Object, mockListAccessRepo.Object, mockCurrentUser.Object);

        // Act
        var result = await service.MarkTodoItemAsCompleteAsync(item.Id);

        // Assert
        result.Should().NotBeNull();
        mockTodoItemRepo.Verify(r => r.MarkTodoItemAsComplete(item), Times.Once);
        mockTodoItemRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task MarkTodoItemAsCompleteAsync_ThrowsMissingValue_WhenEmptyGuid()
    {
        // Arrange
        var mockTodoListRepo = new Mock<ITodoListRepository>();
        var mockTodoItemRepo = new Mock<ITodoItemRepository>();
        var mockListAccessRepo = new Mock<IListAccessRepository>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        var service = new TodoItemService(mockTodoListRepo.Object, mockTodoItemRepo.Object, mockListAccessRepo.Object, mockCurrentUser.Object);

        // Act & Assert
        await service.Invoking(s => s.MarkTodoItemAsCompleteAsync(Guid.Empty))
            .Should().ThrowAsync<MissingValueException>();
    }

    [Fact]
    public async Task MarkTodoItemAsCompleteAsync_ThrowsNotFound_WhenItemNotExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        var mockTodoListRepo = new Mock<ITodoListRepository>();
        var mockTodoItemRepo = new Mock<ITodoItemRepository>();
        var mockListAccessRepo = new Mock<IListAccessRepository>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        mockCurrentUser.Setup(c => c.GetUserId()).Returns(userId);
        mockTodoItemRepo.Setup(r => r.GetTodoItemByIdAsync(itemId)).ReturnsAsync((TodoItem?)null);

        var service = new TodoItemService(mockTodoListRepo.Object, mockTodoItemRepo.Object, mockListAccessRepo.Object, mockCurrentUser.Object);

        // Act & Assert
        await service.Invoking(s => s.MarkTodoItemAsCompleteAsync(itemId))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task MarkTodoItemAsCompleteAsync_ThrowsAuth_WhenUserHasNoAccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var list = TestData.TodoList().WithOwnerId(userId).Build();
        var item = TestData.TodoItem().WithTodoListId(list.Id).Build();

        var mockTodoListRepo = new Mock<ITodoListRepository>();
        var mockTodoItemRepo = new Mock<ITodoItemRepository>();
        var mockListAccessRepo = new Mock<IListAccessRepository>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        mockCurrentUser.Setup(c => c.GetUserId()).Returns(userId);
        mockTodoItemRepo.Setup(r => r.GetTodoItemByIdAsync(item.Id)).ReturnsAsync(item);
        mockListAccessRepo.Setup(r => r.UserHasAccessToListAsync(userId, list.Id)).ReturnsAsync(false);

        var service = new TodoItemService(mockTodoListRepo.Object, mockTodoItemRepo.Object, mockListAccessRepo.Object, mockCurrentUser.Object);

        // Act & Assert
        await service.Invoking(s => s.MarkTodoItemAsCompleteAsync(item.Id))
            .Should().ThrowAsync<AuthException>();
    }

    [Fact]
    public async Task MarkTodoItemAsCompleteAsync_ThrowsBusinessLogic_WhenAlreadyCompleted()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var list = TestData.TodoList().WithOwnerId(userId).Build();
        var item = TestData.TodoItem().WithTodoListId(list.Id).AsCompleted().Build();

        var mockTodoListRepo = new Mock<ITodoListRepository>();
        var mockTodoItemRepo = new Mock<ITodoItemRepository>();
        var mockListAccessRepo = new Mock<IListAccessRepository>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        mockCurrentUser.Setup(c => c.GetUserId()).Returns(userId);
        mockTodoItemRepo.Setup(r => r.GetTodoItemByIdAsync(item.Id)).ReturnsAsync(item);
        mockListAccessRepo.Setup(r => r.UserHasAccessToListAsync(userId, list.Id)).ReturnsAsync(true);

        var service = new TodoItemService(mockTodoListRepo.Object, mockTodoItemRepo.Object, mockListAccessRepo.Object, mockCurrentUser.Object);

        // Act & Assert
        await service.Invoking(s => s.MarkTodoItemAsCompleteAsync(item.Id))
            .Should().ThrowAsync<BusinessLogicException>();
    }

    #endregion

    #region MarkTodoItemAsIncompleteAsync Tests

    [Fact]
    public async Task MarkTodoItemAsIncompleteAsync_ReturnsTodoItemDto_WhenValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var list = TestData.TodoList().WithOwnerId(userId).Build();
        var item = TestData.TodoItem().WithTodoListId(list.Id).AsCompleted().Build();

        var mockTodoListRepo = new Mock<ITodoListRepository>();
        var mockTodoItemRepo = new Mock<ITodoItemRepository>();
        var mockListAccessRepo = new Mock<IListAccessRepository>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        mockCurrentUser.Setup(c => c.GetUserId()).Returns(userId);
        mockTodoItemRepo.Setup(r => r.GetTodoItemByIdAsync(item.Id)).ReturnsAsync(item);
        mockListAccessRepo.Setup(r => r.UserHasAccessToListAsync(userId, list.Id)).ReturnsAsync(true);

        var service = new TodoItemService(mockTodoListRepo.Object, mockTodoItemRepo.Object, mockListAccessRepo.Object, mockCurrentUser.Object);

        // Act
        var result = await service.MarkTodoItemAsIncompleteAsync(item.Id);

        // Assert
        result.Should().NotBeNull();
        mockTodoItemRepo.Verify(r => r.MarkTodoItemAsIncomplete(item), Times.Once);
        mockTodoItemRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task MarkTodoItemAsIncompleteAsync_ThrowsMissingValue_WhenEmptyGuid()
    {
        // Arrange
        var mockTodoListRepo = new Mock<ITodoListRepository>();
        var mockTodoItemRepo = new Mock<ITodoItemRepository>();
        var mockListAccessRepo = new Mock<IListAccessRepository>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        var service = new TodoItemService(mockTodoListRepo.Object, mockTodoItemRepo.Object, mockListAccessRepo.Object, mockCurrentUser.Object);

        // Act & Assert
        await service.Invoking(s => s.MarkTodoItemAsIncompleteAsync(Guid.Empty))
            .Should().ThrowAsync<MissingValueException>();
    }

    [Fact]
    public async Task MarkTodoItemAsIncompleteAsync_ThrowsBusinessLogic_WhenNotCompleted()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var list = TestData.TodoList().WithOwnerId(userId).Build();
        var item = TestData.TodoItem().WithTodoListId(list.Id).AsIncomplete().Build();

        var mockTodoListRepo = new Mock<ITodoListRepository>();
        var mockTodoItemRepo = new Mock<ITodoItemRepository>();
        var mockListAccessRepo = new Mock<IListAccessRepository>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        mockCurrentUser.Setup(c => c.GetUserId()).Returns(userId);
        mockTodoItemRepo.Setup(r => r.GetTodoItemByIdAsync(item.Id)).ReturnsAsync(item);
        mockListAccessRepo.Setup(r => r.UserHasAccessToListAsync(userId, list.Id)).ReturnsAsync(true);

        var service = new TodoItemService(mockTodoListRepo.Object, mockTodoItemRepo.Object, mockListAccessRepo.Object, mockCurrentUser.Object);

        // Act & Assert
        await service.Invoking(s => s.MarkTodoItemAsIncompleteAsync(item.Id))
            .Should().ThrowAsync<BusinessLogicException>();
    }

    #endregion

    #region DeleteTodoItem Tests

    [Fact]
    public async Task DeleteTodoItem_DeletesSuccessfully_WhenUserHasAccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var list = TestData.TodoList().WithOwnerId(userId).Build();
        var item = TestData.TodoItem().WithTodoListId(list.Id).Build();

        var mockTodoListRepo = new Mock<ITodoListRepository>();
        var mockTodoItemRepo = new Mock<ITodoItemRepository>();
        var mockListAccessRepo = new Mock<IListAccessRepository>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        mockCurrentUser.Setup(c => c.GetUserId()).Returns(userId);
        mockTodoItemRepo.Setup(r => r.GetTodoItemByIdAsync(item.Id)).ReturnsAsync(item);
        mockListAccessRepo.Setup(r => r.UserHasAccessToListAsync(userId, list.Id)).ReturnsAsync(true);

        var service = new TodoItemService(mockTodoListRepo.Object, mockTodoItemRepo.Object, mockListAccessRepo.Object, mockCurrentUser.Object);

        // Act
        await service.DeleteTodoItem(item.Id);

        // Assert
        mockTodoItemRepo.Verify(r => r.DeleteTodoItem(item), Times.Once);
        mockTodoItemRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteTodoItem_ThrowsNotFound_WhenItemNotExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        var mockTodoListRepo = new Mock<ITodoListRepository>();
        var mockTodoItemRepo = new Mock<ITodoItemRepository>();
        var mockListAccessRepo = new Mock<IListAccessRepository>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        mockCurrentUser.Setup(c => c.GetUserId()).Returns(userId);
        mockTodoItemRepo.Setup(r => r.GetTodoItemByIdAsync(itemId)).ReturnsAsync((TodoItem?)null);

        var service = new TodoItemService(mockTodoListRepo.Object, mockTodoItemRepo.Object, mockListAccessRepo.Object, mockCurrentUser.Object);

        // Act & Assert
        await service.Invoking(s => s.DeleteTodoItem(itemId))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteTodoItem_ThrowsAuth_WhenUserHasNoAccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var list = TestData.TodoList().WithOwnerId(userId).Build();
        var item = TestData.TodoItem().WithTodoListId(list.Id).Build();

        var mockTodoListRepo = new Mock<ITodoListRepository>();
        var mockTodoItemRepo = new Mock<ITodoItemRepository>();
        var mockListAccessRepo = new Mock<IListAccessRepository>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        mockCurrentUser.Setup(c => c.GetUserId()).Returns(userId);
        mockTodoItemRepo.Setup(r => r.GetTodoItemByIdAsync(item.Id)).ReturnsAsync(item);
        mockListAccessRepo.Setup(r => r.UserHasAccessToListAsync(userId, list.Id)).ReturnsAsync(false);

        var service = new TodoItemService(mockTodoListRepo.Object, mockTodoItemRepo.Object, mockListAccessRepo.Object, mockCurrentUser.Object);

        // Act & Assert
        await service.Invoking(s => s.DeleteTodoItem(item.Id))
            .Should().ThrowAsync<AuthException>();
    }

    #endregion
}
