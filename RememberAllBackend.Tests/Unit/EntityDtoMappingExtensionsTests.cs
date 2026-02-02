using FluentAssertions;
using RememberAll.src.DTOs;
using RememberAll.src.DTOs.Create;
using RememberAll.src.Entities;
using RememberAll.src.Extensions;
using RememberAllBackend.Tests.Helpers;

namespace RememberAllBackend.Tests.Unit;

public class EntityDtoMappingExtensionsTests
{
    #region User Mapping Tests

    [Fact]
    public void CreateUserDto_ToEntity_MapsCorrectly()
    {
        // Arrange
        var createUserDto = TestData.CreateUserDto("Alice", "alice@example.com", "Pass123!@#");

        // Act
        var user = createUserDto.ToEntity();

        // Assert
        user.Should().NotBeNull();
        user.Name.Should().Be("Alice");
        user.Email.Should().Be("alice@example.com");
        user.Id.Should().Be(Guid.Empty); // New entity should have empty Guid
    }

    [Fact]
    public void UserDto_ToEntity_MapsCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var userDto = new UserDto(userId, "Bob", "bob@example.com");

        // Act
        var user = userDto.ToEntity();

        // Assert
        user.Should().NotBeNull();
        user.Id.Should().Be(userId);
        user.Name.Should().Be("Bob");
        user.Email.Should().Be("bob@example.com");
    }

    [Fact]
    public void User_ToDto_MapsCorrectly()
    {
        // Arrange
        var user = TestData.User()
            .WithName("Charlie")
            .WithEmail("charlie@example.com")
            .Build();

        // Act
        var userDto = user.ToDto();

        // Assert
        userDto.Should().NotBeNull();
        userDto.Id.Should().Be(user.Id);
        userDto.Name.Should().Be("Charlie");
        userDto.Email.Should().Be("charlie@example.com");
    }

    #endregion

    #region TodoList Mapping Tests

    [Fact]
    public void CreateTodoListDto_ToEntity_MapsCorrectlyWithOwner()
    {
        // Arrange
        var owner = TestData.User().Build();
        var createTodoListDto = TestData.CreateTodoListDto("Shopping");

        // Act
        var todoList = createTodoListDto.ToEntity(owner);

        // Assert
        todoList.Should().NotBeNull();
        todoList.Name.Should().Be("Shopping");
        todoList.OwnerId.Should().Be(owner.Id);
        todoList.Id.Should().Be(Guid.Empty); // New entity should have empty Guid
    }

    [Fact]
    public void TodoList_ToDto_MapsCorrectly()
    {
        // Arrange
        var owner = TestData.User().Build();
        var todoList = TestData.TodoList()
            .WithOwnerId(owner.Id)
            .WithName("Work Items")
            .WithItems(2)
            .Build();

        // Act
        var todoListDto = todoList.ToDto();

        // Assert
        todoListDto.Should().NotBeNull();
        todoListDto.Id.Should().Be(todoList.Id);
        todoListDto.OwnerId.Should().Be(owner.Id);
        todoListDto.Name.Should().Be("Work Items");
        todoListDto.Items.Should().HaveCount(2);
        todoListDto.Items.Should().AllSatisfy(item => item.Should().NotBeNull());
    }



    #endregion

    #region TodoItem Mapping Tests

    [Fact]
    public void CreateTodoItemDto_ToEntity_MapsCorrectlyWithTodoList()
    {
        // Arrange
        var owner = TestData.User().Build();
        var todoList = TestData.TodoList().WithOwnerId(owner.Id).Build();
        var createTodoItemDto = TestData.CreateTodoItemDto(todoList.Id, "Buy milk");

        // Act
        var todoItem = createTodoItemDto.ToEntity(todoList);

        // Assert
        todoItem.Should().NotBeNull();
        todoItem.Text.Should().Be("Buy milk");
        todoItem.TodoListId.Should().Be(createTodoItemDto.TodoListId); // Uses DTO's TodoListId, not todoList.Id
        todoItem.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void TodoItem_ToDto_MapsCorrectly()
    {
        // Arrange
        var owner = TestData.User().Build();
        var todoList = TestData.TodoList().WithOwnerId(owner.Id).Build();
        var todoItem = TestData.TodoItem()
            .WithTodoListId(todoList.Id)
            .WithText("Complete task")
            .Build();

        // Act
        var todoItemDto = todoItem.ToDto();

        // Assert
        todoItemDto.Should().NotBeNull();
        todoItemDto.Id.Should().Be(todoItem.Id);
        todoItemDto.Text.Should().Be("Complete task");
        todoItemDto.IsCompleted.Should().BeFalse();
        todoItemDto.CompletionCount.Should().Be(0);
    }

    [Fact]
    public void CompletedTodoItem_ToDto_MapsCorrectly()
    {
        // Arrange
        var owner = TestData.User().Build();
        var todoList = TestData.TodoList().WithOwnerId(owner.Id).Build();
        var todoItem = TestData.TodoItem()
            .WithTodoListId(todoList.Id)
            .WithText("Done task")
            .AsCompleted()
            .Build();

        // Act
        var todoItemDto = todoItem.ToDto();

        // Assert
        todoItemDto.Should().NotBeNull();
        todoItemDto.IsCompleted.Should().BeTrue();
    }

    #endregion

    #region ListAccess Mapping Tests

    [Fact]
    public void CreateListAccessDto_ToEntity_MapsCorrectly()
    {
        // Arrange
        var user = TestData.User().Build();
        var owner = TestData.User().Build();
        var todoList = TestData.TodoList().WithOwnerId(owner.Id).Build();
        var createListAccessDto = new CreateListAccessDto(user.Id, todoList.Id);

        // Act
        var listAccess = createListAccessDto.ToEntity(user, todoList);

        // Assert
        listAccess.Should().NotBeNull();
        listAccess.UserId.Should().Be(user.Id);
        listAccess.ListId.Should().Be(todoList.Id);
        listAccess.Id.Should().Be(Guid.Empty); // New entity should have empty Guid
    }

    [Fact]
    public void ListAccessDto_ToEntity_MapsCorrectly()
    {
        // Arrange
        var user = TestData.User().Build();
        var owner = TestData.User().Build();
        var todoList = TestData.TodoList().WithOwnerId(owner.Id).Build();
        var listAccessId = Guid.NewGuid();
        var listAccessDto = new ListAccessDto(listAccessId, user.Id, "Alice", todoList.Id, "Shopping");

        // Act
        var listAccess = listAccessDto.ToEntity(user, todoList);

        // Assert
        listAccess.Should().NotBeNull();
        listAccess.Id.Should().Be(listAccessId);
        listAccess.UserId.Should().Be(user.Id);
        listAccess.ListId.Should().Be(todoList.Id);
    }

    [Fact]
    public void ListAccess_ToDto_MapsCorrectly()
    {
        // Arrange
        var user = TestData.User().WithName("David").Build();
        var owner = TestData.User().Build();
        var todoList = TestData.TodoList().WithOwnerId(owner.Id).WithName("Shared List").Build();

        var listAccess = new ListAccess
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            ListId = todoList.Id,
            List = todoList
        };

        // Act
        var listAccessDto = listAccess.ToDto();

        // Assert
        listAccessDto.Should().NotBeNull();
        listAccessDto.Id.Should().Be(listAccess.Id);
        listAccessDto.UserId.Should().Be(user.Id);
        listAccessDto.UserName.Should().Be("David");
        listAccessDto.ListId.Should().Be(todoList.Id);
        listAccessDto.ListName.Should().Be("Shared List");
    }

    #endregion

    #region Invite Mapping Tests

    [Fact]
    public void CreateInviteDto_ToEntity_MapsCorrectly()
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var receiverId = Guid.NewGuid();
        var listId = Guid.NewGuid();
        var createInviteDto = new CreateInviteDto(receiverId, listId);

        // Act
        var invite = createInviteDto.ToEntity(senderId);

        // Assert
        invite.Should().NotBeNull();
        invite.InviteSenderId.Should().Be(senderId);
        invite.InviteRecieverId.Should().Be(receiverId);
        invite.ListId.Should().Be(listId);
        invite.Id.Should().Be(Guid.Empty); // New entity should have empty Guid
    }

    [Fact]
    public void Invite_ToDto_MapsCorrectly()
    {
        // Arrange
        var sender = TestData.User().WithName("Grace").Build();
        var receiver = TestData.User().WithName("Henry").Build();
        var owner = TestData.User().Build();
        var todoList = TestData.TodoList().WithOwnerId(owner.Id).WithName("Project").Build();

        var invite = new Invite
        {
            Id = Guid.NewGuid(),
            InviteSenderId = sender.Id,
            InviteSender = sender,
            InviteRecieverId = receiver.Id,
            InviteReciever = receiver,
            ListId = todoList.Id,
            List = todoList
        };

        // Act
        var inviteDto = invite.ToDto();

        // Assert
        inviteDto.Should().NotBeNull();
        inviteDto.Id.Should().Be(invite.Id);
        inviteDto.InviteSenderId.Should().Be(sender.Id);
        inviteDto.InviteSenderName.Should().Be("Grace");
        inviteDto.InviteRecieverId.Should().Be(receiver.Id);
        inviteDto.InviteRecieverName.Should().Be("Henry");
        inviteDto.ListId.Should().Be(todoList.Id);
        inviteDto.ListName.Should().Be("Project");
    }

    [Fact]
    public void Invite_ToListAccess_MapsCorrectly()
    {
        // Arrange
        var sender = TestData.User().Build();
        var receiver = TestData.User().Build();
        var owner = TestData.User().Build();
        var todoList = TestData.TodoList().WithOwnerId(owner.Id).Build();

        var invite = new Invite
        {
            Id = Guid.NewGuid(),
            InviteSenderId = sender.Id,
            InviteRecieverId = receiver.Id,
            ListId = todoList.Id
        };

        // Act
        var listAccess = invite.ToListAccess();

        // Assert
        listAccess.Should().NotBeNull();
        listAccess.UserId.Should().Be(receiver.Id);
        listAccess.ListId.Should().Be(todoList.Id);
        listAccess.Id.Should().Be(Guid.Empty); // New entity should have empty Guid
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void ApplyNonNullValuesFromDto_WithAllNonNullValues_UpdatesEntity()
    {
        // Arrange
        var user = TestData.User().WithName("OldName").WithEmail("old@example.com").Build();
        var userDto = new UserDto(user.Id, "NewName", "new@example.com");

        // Act
        user.ApplyNonNullValuesFromDto(userDto);

        // Assert
        user.Name.Should().Be("NewName");
        user.Email.Should().Be("new@example.com");
    }

    [Fact]
    public void ApplyNonNullValuesFromDto_WithNullValues_DoesNotUpdateProperties()
    {
        // Arrange
        var user = TestData.User().WithName("Original").WithEmail("original@example.com").Build();
        var userDto = new UserDto(user.Id, null, null);

        // Act
        user.ApplyNonNullValuesFromDto(userDto);

        // Assert
        user.Name.Should().Be("Original");
        user.Email.Should().Be("original@example.com");
    }

    [Fact]
    public void ApplyNonNullValuesFromDto_WithPartialNullValues_UpdatesOnlyNonNullValues()
    {
        // Arrange
        var user = TestData.User().WithName("OldName").WithEmail("old@example.com").Build();
        var userDto = new UserDto(user.Id, "NewName", null);

        // Act
        user.ApplyNonNullValuesFromDto(userDto);

        // Assert
        user.Name.Should().Be("NewName");
        user.Email.Should().Be("old@example.com");
    }

    #endregion
}