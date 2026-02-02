using FluentAssertions;
using Moq;
using RememberAll.src.DTOs.Create;
using RememberAll.src.Entities;
using RememberAll.src.Exceptions;
using RememberAll.src.Repositories.Interfaces;
using RememberAll.src.Services;
using RememberAll.src.Services.Interfaces;
using RememberAllBackend.Tests.Helpers;

namespace RememberAllBackend.Tests.Unit.Services;

public class InviteServiceTests
{
    #region CreateInviteAsync Tests

    [Fact]
    public async Task CreateInviteAsync_ReturnsInviteDto_WhenValid()
    {
        // Arrange
        var sender = TestData.User().Build();
        var receiver = TestData.User().Build();
        var list = TestData.TodoList().WithOwnerId(sender.Id).Build();
        var invite = new Invite
        {
            Id = Guid.NewGuid(),
            ListId = list.Id,
            InviteSenderId = sender.Id,
            InviteRecieverId = receiver.Id,
            InviteSender = sender,
            InviteReciever = receiver,
            List = list
        };

        var mockUserRepo = new Mock<IUserRepository>();
        var mockTodoListRepo = new Mock<ITodoListRepository>();
        var mockInviteRepo = new Mock<IInviteRepository>();
        var mockListAccessRepo = new Mock<IListAccessRepository>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        mockCurrentUser.Setup(c => c.GetUserId()).Returns(sender.Id);
        mockUserRepo.Setup(r => r.UserExistsByIdAsync(sender.Id)).ReturnsAsync(true);
        mockUserRepo.Setup(r => r.UserExistsByIdAsync(receiver.Id)).ReturnsAsync(true);
        mockTodoListRepo.Setup(r => r.GetTodoListByIdAsync(list.Id)).ReturnsAsync(list);
        mockListAccessRepo.Setup(r => r.UserHasAccessToListAsync(sender.Id, list.Id)).ReturnsAsync(true);
        mockInviteRepo.Setup(r => r.CreateInviteAsync(It.IsAny<Invite>())).ReturnsAsync(invite);

        var service = new InviteService(mockUserRepo.Object, mockTodoListRepo.Object, mockInviteRepo.Object, mockListAccessRepo.Object, mockCurrentUser.Object);
        var createDto = new CreateInviteDto(receiver.Id, list.Id);

        // Act
        var result = await service.CreateInviteAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.InviteRecieverId.Should().Be(receiver.Id);
        result.ListId.Should().Be(list.Id);
        mockInviteRepo.Verify(r => r.CreateInviteAsync(It.IsAny<Invite>()), Times.Once);
        mockInviteRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateInviteAsync_ThrowsNotFound_WhenSenderNotExists()
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var receiverId = Guid.NewGuid();
        var listId = Guid.NewGuid();

        var mockUserRepo = new Mock<IUserRepository>();
        var mockTodoListRepo = new Mock<ITodoListRepository>();
        var mockInviteRepo = new Mock<IInviteRepository>();
        var mockListAccessRepo = new Mock<IListAccessRepository>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        mockCurrentUser.Setup(c => c.GetUserId()).Returns(senderId);
        mockUserRepo.Setup(r => r.UserExistsByIdAsync(senderId)).ReturnsAsync(false);

        var service = new InviteService(mockUserRepo.Object, mockTodoListRepo.Object, mockInviteRepo.Object, mockListAccessRepo.Object, mockCurrentUser.Object);
        var createDto = new CreateInviteDto(receiverId, listId);

        // Act & Assert
        await service.Invoking(s => s.CreateInviteAsync(createDto))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateInviteAsync_ThrowsNotFound_WhenReceiverNotExists()
    {
        // Arrange
        var sender = TestData.User().Build();
        var receiverId = Guid.NewGuid();
        var listId = Guid.NewGuid();

        var mockUserRepo = new Mock<IUserRepository>();
        var mockTodoListRepo = new Mock<ITodoListRepository>();
        var mockInviteRepo = new Mock<IInviteRepository>();
        var mockListAccessRepo = new Mock<IListAccessRepository>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        mockCurrentUser.Setup(c => c.GetUserId()).Returns(sender.Id);
        mockUserRepo.Setup(r => r.UserExistsByIdAsync(sender.Id)).ReturnsAsync(true);
        mockUserRepo.Setup(r => r.UserExistsByIdAsync(receiverId)).ReturnsAsync(false);

        var service = new InviteService(mockUserRepo.Object, mockTodoListRepo.Object, mockInviteRepo.Object, mockListAccessRepo.Object, mockCurrentUser.Object);
        var createDto = new CreateInviteDto(receiverId, listId);

        // Act & Assert
        await service.Invoking(s => s.CreateInviteAsync(createDto))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateInviteAsync_ThrowsNotFound_WhenListNotExists()
    {
        // Arrange
        var sender = TestData.User().Build();
        var receiver = TestData.User().Build();
        var listId = Guid.NewGuid();

        var mockUserRepo = new Mock<IUserRepository>();
        var mockTodoListRepo = new Mock<ITodoListRepository>();
        var mockInviteRepo = new Mock<IInviteRepository>();
        var mockListAccessRepo = new Mock<IListAccessRepository>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        mockCurrentUser.Setup(c => c.GetUserId()).Returns(sender.Id);
        mockUserRepo.Setup(r => r.UserExistsByIdAsync(sender.Id)).ReturnsAsync(true);
        mockUserRepo.Setup(r => r.UserExistsByIdAsync(receiver.Id)).ReturnsAsync(true);
        mockTodoListRepo.Setup(r => r.GetTodoListByIdAsync(listId)).ReturnsAsync((TodoList?)null);

        var service = new InviteService(mockUserRepo.Object, mockTodoListRepo.Object, mockInviteRepo.Object, mockListAccessRepo.Object, mockCurrentUser.Object);
        var createDto = new CreateInviteDto(receiver.Id, listId);

        // Act & Assert
        await service.Invoking(s => s.CreateInviteAsync(createDto))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateInviteAsync_ThrowsAuth_WhenSenderHasNoAccess()
    {
        // Arrange
        var sender = TestData.User().Build();
        var receiver = TestData.User().Build();
        var list = TestData.TodoList().WithOwnerId(sender.Id).Build();

        var mockUserRepo = new Mock<IUserRepository>();
        var mockTodoListRepo = new Mock<ITodoListRepository>();
        var mockInviteRepo = new Mock<IInviteRepository>();
        var mockListAccessRepo = new Mock<IListAccessRepository>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        mockCurrentUser.Setup(c => c.GetUserId()).Returns(sender.Id);
        mockUserRepo.Setup(r => r.UserExistsByIdAsync(sender.Id)).ReturnsAsync(true);
        mockUserRepo.Setup(r => r.UserExistsByIdAsync(receiver.Id)).ReturnsAsync(true);
        mockTodoListRepo.Setup(r => r.GetTodoListByIdAsync(list.Id)).ReturnsAsync(list);
        mockListAccessRepo.Setup(r => r.UserHasAccessToListAsync(sender.Id, list.Id)).ReturnsAsync(false);

        var service = new InviteService(mockUserRepo.Object, mockTodoListRepo.Object, mockInviteRepo.Object, mockListAccessRepo.Object, mockCurrentUser.Object);
        var createDto = new CreateInviteDto(receiver.Id, list.Id);

        // Act & Assert
        await service.Invoking(s => s.CreateInviteAsync(createDto))
            .Should().ThrowAsync<AuthException>();
    }

    #endregion

    #region GetReceivedInvitesByUserAsync Tests

    [Fact]
    public async Task GetReceivedInvitesByUserAsync_ReturnsInvites_WhenValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var sender = TestData.User().Build();
        var list = TestData.TodoList().WithOwnerId(sender.Id).Build();
        var invites = new List<Invite> {
            new Invite {
                Id = Guid.NewGuid(),
                InviteRecieverId = userId,
                InviteSenderId = sender.Id,
                ListId = list.Id,
                InviteSender = sender,
                InviteReciever = TestData.User().WithId(userId).Build(),
                List = list
            }
        };

        var mockUserRepo = new Mock<IUserRepository>();
        var mockTodoListRepo = new Mock<ITodoListRepository>();
        var mockInviteRepo = new Mock<IInviteRepository>();
        var mockListAccessRepo = new Mock<IListAccessRepository>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        mockCurrentUser.Setup(c => c.GetUserId()).Returns(userId);
        mockUserRepo.Setup(r => r.UserExistsByIdAsync(userId)).ReturnsAsync(true);
        mockInviteRepo.Setup(r => r.GetRecievedInvitesByUserId(userId)).ReturnsAsync(invites);

        var service = new InviteService(mockUserRepo.Object, mockTodoListRepo.Object, mockInviteRepo.Object, mockListAccessRepo.Object, mockCurrentUser.Object);

        // Act
        var result = await service.GetReceivedInvitesByUserAsync();

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetReceivedInvitesByUserAsync_ThrowsNotFound_WhenUserNotExists()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var mockUserRepo = new Mock<IUserRepository>();
        var mockTodoListRepo = new Mock<ITodoListRepository>();
        var mockInviteRepo = new Mock<IInviteRepository>();
        var mockListAccessRepo = new Mock<IListAccessRepository>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        mockCurrentUser.Setup(c => c.GetUserId()).Returns(userId);
        mockUserRepo.Setup(r => r.UserExistsByIdAsync(userId)).ReturnsAsync(false);

        var service = new InviteService(mockUserRepo.Object, mockTodoListRepo.Object, mockInviteRepo.Object, mockListAccessRepo.Object, mockCurrentUser.Object);

        // Act & Assert
        await service.Invoking(s => s.GetReceivedInvitesByUserAsync())
            .Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetSentInvitesByUserAsync Tests

    [Fact]
    public async Task GetSentInvitesByUserAsync_ReturnsInvites_WhenValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var receiver = TestData.User().Build();
        var list = TestData.TodoList().WithOwnerId(userId).Build();
        var invites = new List<Invite> {
            new Invite {
                Id = Guid.NewGuid(),
                InviteSenderId = userId,
                InviteRecieverId = receiver.Id,
                ListId = list.Id,
                InviteSender = TestData.User().WithId(userId).Build(),
                InviteReciever = receiver,
                List = list
            }
        };

        var mockUserRepo = new Mock<IUserRepository>();
        var mockTodoListRepo = new Mock<ITodoListRepository>();
        var mockInviteRepo = new Mock<IInviteRepository>();
        var mockListAccessRepo = new Mock<IListAccessRepository>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        mockCurrentUser.Setup(c => c.GetUserId()).Returns(userId);
        mockUserRepo.Setup(r => r.UserExistsByIdAsync(userId)).ReturnsAsync(true);
        mockInviteRepo.Setup(r => r.GetSentInvitesByUserId(userId)).ReturnsAsync(invites);

        var service = new InviteService(mockUserRepo.Object, mockTodoListRepo.Object, mockInviteRepo.Object, mockListAccessRepo.Object, mockCurrentUser.Object);

        // Act
        var result = await service.GetSentInvitesByUserAsync();

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetSentInvitesByUserAsync_ThrowsNotFound_WhenUserNotExists()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var mockUserRepo = new Mock<IUserRepository>();
        var mockTodoListRepo = new Mock<ITodoListRepository>();
        var mockInviteRepo = new Mock<IInviteRepository>();
        var mockListAccessRepo = new Mock<IListAccessRepository>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        mockCurrentUser.Setup(c => c.GetUserId()).Returns(userId);
        mockUserRepo.Setup(r => r.UserExistsByIdAsync(userId)).ReturnsAsync(false);

        var service = new InviteService(mockUserRepo.Object, mockTodoListRepo.Object, mockInviteRepo.Object, mockListAccessRepo.Object, mockCurrentUser.Object);

        // Act & Assert
        await service.Invoking(s => s.GetSentInvitesByUserAsync())
            .Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region AcceptInviteByIdAsync Tests

    [Fact]
    public async Task AcceptInviteByIdAsync_AcceptsInvite_WhenValid()
    {
        // Arrange
        var sender = TestData.User().Build();
        var receiver = TestData.User().Build();
        var list = TestData.TodoList().WithOwnerId(sender.Id).Build();
        var invite = new Invite
        {
            Id = Guid.NewGuid(),
            ListId = list.Id,
            InviteSenderId = sender.Id,
            InviteRecieverId = receiver.Id,
            InviteSender = sender,
            InviteReciever = receiver,
            List = list
        };

        var mockUserRepo = new Mock<IUserRepository>();
        var mockTodoListRepo = new Mock<ITodoListRepository>();
        var mockInviteRepo = new Mock<IInviteRepository>();
        var mockListAccessRepo = new Mock<IListAccessRepository>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        mockCurrentUser.Setup(c => c.GetUserId()).Returns(receiver.Id);
        mockCurrentUser.Setup(c => c.IsCurrentUser(receiver.Id)).Returns(true);
        mockInviteRepo.Setup(r => r.GetInviteByIdAsync(invite.Id)).ReturnsAsync(invite);

        var service = new InviteService(mockUserRepo.Object, mockTodoListRepo.Object, mockInviteRepo.Object, mockListAccessRepo.Object, mockCurrentUser.Object);

        // Act
        await service.AcceptInviteByIdAsync(invite.Id);

        // Assert
        mockListAccessRepo.Verify(r => r.CreateListAccessAsync(It.IsAny<ListAccess>()), Times.Once);
        mockInviteRepo.Verify(r => r.DeleteInvite(invite), Times.Once);
        mockInviteRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AcceptInviteByIdAsync_ThrowsMissingValue_WhenEmptyGuid()
    {
        // Arrange
        var mockUserRepo = new Mock<IUserRepository>();
        var mockTodoListRepo = new Mock<ITodoListRepository>();
        var mockInviteRepo = new Mock<IInviteRepository>();
        var mockListAccessRepo = new Mock<IListAccessRepository>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        var service = new InviteService(mockUserRepo.Object, mockTodoListRepo.Object, mockInviteRepo.Object, mockListAccessRepo.Object, mockCurrentUser.Object);

        // Act & Assert
        await service.Invoking(s => s.AcceptInviteByIdAsync(Guid.Empty))
            .Should().ThrowAsync<MissingValueException>();
    }

    [Fact]
    public async Task AcceptInviteByIdAsync_ThrowsNotFound_WhenInviteNotExists()
    {
        // Arrange
        var inviteId = Guid.NewGuid();

        var mockUserRepo = new Mock<IUserRepository>();
        var mockTodoListRepo = new Mock<ITodoListRepository>();
        var mockInviteRepo = new Mock<IInviteRepository>();
        var mockListAccessRepo = new Mock<IListAccessRepository>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        mockInviteRepo.Setup(r => r.GetInviteByIdAsync(inviteId)).ReturnsAsync((Invite?)null);

        var service = new InviteService(mockUserRepo.Object, mockTodoListRepo.Object, mockInviteRepo.Object, mockListAccessRepo.Object, mockCurrentUser.Object);

        // Act & Assert
        await service.Invoking(s => s.AcceptInviteByIdAsync(inviteId))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task AcceptInviteByIdAsync_ThrowsAuth_WhenNotRecipient()
    {
        // Arrange
        var sender = TestData.User().Build();
        var receiver = TestData.User().Build();
        var otherUser = TestData.User().Build();
        var list = TestData.TodoList().WithOwnerId(sender.Id).Build();
        var invite = new Invite
        {
            Id = Guid.NewGuid(),
            ListId = list.Id,
            InviteSenderId = sender.Id,
            InviteRecieverId = receiver.Id,
            InviteSender = sender,
            InviteReciever = receiver,
            List = list
        };

        var mockUserRepo = new Mock<IUserRepository>();
        var mockTodoListRepo = new Mock<ITodoListRepository>();
        var mockInviteRepo = new Mock<IInviteRepository>();
        var mockListAccessRepo = new Mock<IListAccessRepository>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        mockCurrentUser.Setup(c => c.GetUserId()).Returns(otherUser.Id);
        mockCurrentUser.Setup(c => c.IsCurrentUser(otherUser.Id)).Returns(true);
        mockCurrentUser.Setup(c => c.IsCurrentUser(receiver.Id)).Returns(false);
        mockInviteRepo.Setup(r => r.GetInviteByIdAsync(invite.Id)).ReturnsAsync(invite);

        var service = new InviteService(mockUserRepo.Object, mockTodoListRepo.Object, mockInviteRepo.Object, mockListAccessRepo.Object, mockCurrentUser.Object);

        // Act & Assert
        await service.Invoking(s => s.AcceptInviteByIdAsync(invite.Id))
            .Should().ThrowAsync<AuthException>();
    }

    #endregion

    #region DeleteInviteByIdAsync Tests

    [Fact]
    public async Task DeleteInviteByIdAsync_DeletesInvite_WhenBySender()
    {
        // Arrange
        var sender = TestData.User().Build();
        var receiver = TestData.User().Build();
        var list = TestData.TodoList().WithOwnerId(sender.Id).Build();
        var invite = new Invite
        {
            Id = Guid.NewGuid(),
            ListId = list.Id,
            InviteSenderId = sender.Id,
            InviteRecieverId = receiver.Id,
            InviteSender = sender,
            InviteReciever = receiver,
            List = list
        };

        var mockUserRepo = new Mock<IUserRepository>();
        var mockTodoListRepo = new Mock<ITodoListRepository>();
        var mockInviteRepo = new Mock<IInviteRepository>();
        var mockListAccessRepo = new Mock<IListAccessRepository>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        mockCurrentUser.Setup(c => c.GetUserId()).Returns(sender.Id);
        mockCurrentUser.Setup(c => c.IsCurrentUser(sender.Id)).Returns(true);
        mockInviteRepo.Setup(r => r.GetInviteByIdAsync(invite.Id)).ReturnsAsync(invite);

        var service = new InviteService(mockUserRepo.Object, mockTodoListRepo.Object, mockInviteRepo.Object, mockListAccessRepo.Object, mockCurrentUser.Object);

        // Act
        await service.DeleteInviteByIdAsync(invite.Id);

        // Assert
        mockInviteRepo.Verify(r => r.DeleteInvite(invite), Times.Once);
        mockInviteRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteInviteByIdAsync_DeletesInvite_WhenByReceiver()
    {
        // Arrange
        var sender = TestData.User().Build();
        var receiver = TestData.User().Build();
        var list = TestData.TodoList().WithOwnerId(sender.Id).Build();
        var invite = new Invite
        {
            Id = Guid.NewGuid(),
            ListId = list.Id,
            InviteSenderId = sender.Id,
            InviteRecieverId = receiver.Id,
            InviteSender = sender,
            InviteReciever = receiver,
            List = list
        };

        var mockUserRepo = new Mock<IUserRepository>();
        var mockTodoListRepo = new Mock<ITodoListRepository>();
        var mockInviteRepo = new Mock<IInviteRepository>();
        var mockListAccessRepo = new Mock<IListAccessRepository>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        mockCurrentUser.Setup(c => c.GetUserId()).Returns(receiver.Id);
        mockCurrentUser.Setup(c => c.IsCurrentUser(receiver.Id)).Returns(true);
        mockInviteRepo.Setup(r => r.GetInviteByIdAsync(invite.Id)).ReturnsAsync(invite);

        var service = new InviteService(mockUserRepo.Object, mockTodoListRepo.Object, mockInviteRepo.Object, mockListAccessRepo.Object, mockCurrentUser.Object);

        // Act
        await service.DeleteInviteByIdAsync(invite.Id);

        // Assert
        mockInviteRepo.Verify(r => r.DeleteInvite(invite), Times.Once);
        mockInviteRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteInviteByIdAsync_ThrowsMissingValue_WhenEmptyGuid()
    {
        // Arrange
        var mockUserRepo = new Mock<IUserRepository>();
        var mockTodoListRepo = new Mock<ITodoListRepository>();
        var mockInviteRepo = new Mock<IInviteRepository>();
        var mockListAccessRepo = new Mock<IListAccessRepository>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        var service = new InviteService(mockUserRepo.Object, mockTodoListRepo.Object, mockInviteRepo.Object, mockListAccessRepo.Object, mockCurrentUser.Object);

        // Act & Assert
        await service.Invoking(s => s.DeleteInviteByIdAsync(Guid.Empty))
            .Should().ThrowAsync<MissingValueException>();
    }

    [Fact]
    public async Task DeleteInviteByIdAsync_ThrowsNotFound_WhenInviteNotExists()
    {
        // Arrange
        var inviteId = Guid.NewGuid();

        var mockUserRepo = new Mock<IUserRepository>();
        var mockTodoListRepo = new Mock<ITodoListRepository>();
        var mockInviteRepo = new Mock<IInviteRepository>();
        var mockListAccessRepo = new Mock<IListAccessRepository>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        mockInviteRepo.Setup(r => r.GetInviteByIdAsync(inviteId)).ReturnsAsync((Invite?)null);

        var service = new InviteService(mockUserRepo.Object, mockTodoListRepo.Object, mockInviteRepo.Object, mockListAccessRepo.Object, mockCurrentUser.Object);

        // Act & Assert
        await service.Invoking(s => s.DeleteInviteByIdAsync(inviteId))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteInviteByIdAsync_ThrowsAuth_WhenUnauthorized()
    {
        // Arrange
        var sender = TestData.User().Build();
        var receiver = TestData.User().Build();
        var otherUser = TestData.User().Build();
        var list = TestData.TodoList().WithOwnerId(sender.Id).Build();
        var invite = new Invite
        {
            Id = Guid.NewGuid(),
            ListId = list.Id,
            InviteSenderId = sender.Id,
            InviteRecieverId = receiver.Id,
            InviteSender = sender,
            InviteReciever = receiver,
            List = list
        };

        var mockUserRepo = new Mock<IUserRepository>();
        var mockTodoListRepo = new Mock<ITodoListRepository>();
        var mockInviteRepo = new Mock<IInviteRepository>();
        var mockListAccessRepo = new Mock<IListAccessRepository>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        mockCurrentUser.Setup(c => c.GetUserId()).Returns(otherUser.Id);
        mockCurrentUser.Setup(c => c.IsCurrentUser(It.IsAny<Guid>())).Returns(false);
        mockInviteRepo.Setup(r => r.GetInviteByIdAsync(invite.Id)).ReturnsAsync(invite);

        var service = new InviteService(mockUserRepo.Object, mockTodoListRepo.Object, mockInviteRepo.Object, mockListAccessRepo.Object, mockCurrentUser.Object);

        // Act & Assert
        await service.Invoking(s => s.DeleteInviteByIdAsync(invite.Id))
            .Should().ThrowAsync<AuthException>();
    }

    #endregion
}
