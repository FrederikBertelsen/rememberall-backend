using FluentAssertions;
using RememberAll.src.DTOs;
using RememberAll.src.DTOs.Create;
using RememberAllBackend.Tests.Base;
using RememberAllBackend.Tests.Fixtures;
using RememberAllBackend.Tests.Helpers;
using System.Net;

namespace RememberAllBackend.Tests.Integration.Api;

[Trait("Category", "Integration")]
public class InviteApiTests(IndividualDatabaseFixture fixture) : BaseIntegrationTest(fixture)
{
    #region Create Invite Tests

    [Fact]
    public async Task CreateInvite_ReturnsDto_WhenValid()
    {
        // Arrange
        var (owner, ownerClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Owner", "owner@example.com", "SecurePass123!@#");
        var (receiver, receiverClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Receiver", "receiver@example.com", "SecurePass123!@#");

        var listResponse = await ownerClient.PostAsJsonAsync("/api/lists", TestData.CreateTodoListDto("Shared Shopping List"));
        var list = await listResponse.GetContentAsync<TodoListDto>();

        var createDto = new CreateInviteDto(receiver.Id, list.Id);

        // Act
        var response = await ownerClient.PostAsJsonAsync("/api/invites", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var invite = await response.GetContentAsync<InviteDto>();
        invite.Should().NotBeNull();
        invite.InviteSenderId.Should().Be(owner.Id);
        invite.InviteRecieverId.Should().Be(receiver.Id);
        invite.ListId.Should().Be(list.Id);
    }

    [Fact]
    public async Task CreateInvite_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var userClient = Fixture.Factory.CreateClient();
        var createDto = new CreateInviteDto(Guid.NewGuid(), Guid.NewGuid());

        // Act
        var response = await userClient.PostAsJsonAsync("/api/invites", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateInvite_WithNonExistentList_ReturnsNotFound()
    {
        // Arrange
        var (owner, ownerClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Owner", "owner@example.com", "SecurePass123!@#");
        var (receiver, receiverClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Receiver", "receiver@example.com", "SecurePass123!@#");

        var createDto = new CreateInviteDto(receiver.Id, Guid.NewGuid());

        // Act
        var response = await ownerClient.PostAsJsonAsync("/api/invites", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateInvite_WithNonExistentReceiver_ReturnsNotFound()
    {
        // Arrange
        var (owner, ownerClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Owner", "owner@example.com", "SecurePass123!@#");

        var listResponse = await ownerClient.PostAsJsonAsync("/api/lists", TestData.CreateTodoListDto("Shared Shopping List"));
        var list = await listResponse.GetContentAsync<TodoListDto>();

        var createDto = new CreateInviteDto(Guid.NewGuid(), list.Id);

        // Act
        var response = await ownerClient.PostAsJsonAsync("/api/invites", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateInvite_WhenUserHasNoAccess_ReturnsForbidden()
    {
        // Arrange
        var (owner, ownerClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Owner", "owner@example.com", "SecurePass123!@#");
        var (other, otherClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Other", "other@example.com", "SecurePass123!@#");
        var (receiver, receiverClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Receiver", "receiver@example.com", "SecurePass123!@#");

        var listResponse = await ownerClient.PostAsJsonAsync("/api/lists", TestData.CreateTodoListDto("Private List"));
        var list = await listResponse.GetContentAsync<TodoListDto>();

        var createDto = new CreateInviteDto(receiver.Id, list.Id);

        // Act
        var response = await otherClient.PostAsJsonAsync("/api/invites", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Get Sent Invites Tests

    [Fact]
    public async Task GetSentInvites_ReturnsInvites_WhenUserHasSent()
    {
        // Arrange
        var (sender, senderClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Sender", "sender@example.com", "SecurePass123!@#");
        var (receiver1, receiver1Client) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Receiver1", "receiver1@example.com", "SecurePass123!@#");
        var (receiver2, receiver2Client) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Receiver2", "receiver2@example.com", "SecurePass123!@#");

        var listResponse = await senderClient.PostAsJsonAsync("/api/lists", TestData.CreateTodoListDto("Shared List"));
        var list = await listResponse.GetContentAsync<TodoListDto>();

        // Create two invites
        await senderClient.PostAsJsonAsync("/api/invites", new CreateInviteDto(receiver1.Id, list.Id));
        await senderClient.PostAsJsonAsync("/api/invites", new CreateInviteDto(receiver2.Id, list.Id));

        // Act
        var response = await senderClient.GetAsync("/api/invites/sent");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var invites = await response.GetContentAsync<ICollection<InviteDto>>();
        invites.Should().NotBeNull();
        invites.Should().HaveCount(2);
        invites.Should().OnlyContain(i => i.InviteSenderId == sender.Id);
        invites.Should().OnlyContain(i => i.ListId == list.Id);
    }

    [Fact]
    public async Task GetSentInvites_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var userClient = Fixture.Factory.CreateClient();

        // Act
        var response = await userClient.GetAsync("/api/invites/sent");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetSentInvites_WhenUserHasNone_ReturnsEmptyCollection()
    {
        // Arrange
        var (user, userClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "User", "user@example.com", "SecurePass123!@#");

        // Act
        var response = await userClient.GetAsync("/api/invites/sent");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var invites = await response.GetContentAsync<ICollection<InviteDto>>();
        invites.Should().NotBeNull();
        invites.Should().BeEmpty();
    }

    #endregion

    #region Get Received Invites Tests

    [Fact]
    public async Task GetReceivedInvites_ReturnsInvites_WhenUserHasReceived()
    {
        // Arrange
        var (sender1, sender1Client) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Sender1", "sender1@example.com", "SecurePass123!@#");
        var (sender2, sender2Client) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Sender2", "sender2@example.com", "SecurePass123!@#");
        var (receiver, receiverClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Receiver", "receiver@example.com", "SecurePass123!@#");

        var list1Response = await sender1Client.PostAsJsonAsync("/api/lists", TestData.CreateTodoListDto("List 1"));
        var list1 = await list1Response.GetContentAsync<TodoListDto>();

        var list2Response = await sender2Client.PostAsJsonAsync("/api/lists", TestData.CreateTodoListDto("List 2"));
        var list2 = await list2Response.GetContentAsync<TodoListDto>();

        // Create two invites to the receiver
        await sender1Client.PostAsJsonAsync("/api/invites", new CreateInviteDto(receiver.Id, list1.Id));
        await sender2Client.PostAsJsonAsync("/api/invites", new CreateInviteDto(receiver.Id, list2.Id));

        // Act
        var response = await receiverClient.GetAsync("/api/invites/received");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var invites = await response.GetContentAsync<ICollection<InviteDto>>();
        invites.Should().NotBeNull();
        invites.Should().HaveCount(2);
        invites.Should().OnlyContain(i => i.InviteRecieverId == receiver.Id);
    }

    [Fact]
    public async Task GetReceivedInvites_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var userClient = Fixture.Factory.CreateClient();

        // Act
        var response = await userClient.GetAsync("/api/invites/received");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetReceivedInvites_WhenUserHasNone_ReturnsEmptyCollection()
    {
        // Arrange
        var (user, userClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "User", "user@example.com", "SecurePass123!@#");

        // Act
        var response = await userClient.GetAsync("/api/invites/received");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var invites = await response.GetContentAsync<ICollection<InviteDto>>();
        invites.Should().NotBeNull();
        invites.Should().BeEmpty();
    }

    #endregion

    #region Accept Invite Tests

    [Fact]
    public async Task AcceptInvite_ReturnsNoContent_WhenValid()
    {
        // Arrange
        var (sender, senderClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Sender", "sender@example.com", "SecurePass123!@#");
        var (receiver, receiverClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Receiver", "receiver@example.com", "SecurePass123!@#");

        var listResponse = await senderClient.PostAsJsonAsync("/api/lists", TestData.CreateTodoListDto("Shared List"));
        var list = await listResponse.GetContentAsync<TodoListDto>();

        var inviteResponse = await senderClient.PostAsJsonAsync("/api/invites", new CreateInviteDto(receiver.Id, list.Id));
        var invite = await inviteResponse.GetContentAsync<InviteDto>();

        // Act
        var response = await receiverClient.PatchAsync($"/api/invites/{invite.Id}/accept", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify invite is removed from received invites
        var receivedInvitesResponse = await receiverClient.GetAsync("/api/invites/received");
        var receivedInvites = await receivedInvitesResponse.GetContentAsync<ICollection<InviteDto>>();
        receivedInvites.Should().BeEmpty();

        // Verify receiver can now access the list
        var accessResponse = await receiverClient.GetAsync($"/api/lists/{list.Id}");
        accessResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AcceptInvite_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var userClient = Fixture.Factory.CreateClient();

        // Act
        var response = await userClient.PatchAsync($"/api/invites/{Guid.NewGuid()}/accept", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AcceptInvite_WithNonExistentInvite_ReturnsNotFound()
    {
        // Arrange
        var (user, userClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "User", "user@example.com", "SecurePass123!@#");

        // Act
        var response = await userClient.PatchAsync($"/api/invites/{Guid.NewGuid()}/accept", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AcceptInvite_WhenNotReceiver_ReturnsForbidden()
    {
        // Arrange
        var (sender, senderClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Sender", "sender@example.com", "SecurePass123!@#");
        var (receiver, receiverClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Receiver", "receiver@example.com", "SecurePass123!@#");
        var (other, otherClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Other", "other@example.com", "SecurePass123!@#");

        var listResponse = await senderClient.PostAsJsonAsync("/api/lists", TestData.CreateTodoListDto("Shared List"));
        var list = await listResponse.GetContentAsync<TodoListDto>();

        var inviteResponse = await senderClient.PostAsJsonAsync("/api/invites", new CreateInviteDto(receiver.Id, list.Id));
        var invite = await inviteResponse.GetContentAsync<InviteDto>();

        // Act
        var response = await otherClient.PatchAsync($"/api/invites/{invite.Id}/accept", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Delete Invite Tests

    [Fact]
    public async Task DeleteInvite_ReturnsNoContent_WhenSenderDeletes()
    {
        // Arrange
        var (sender, senderClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Sender", "sender@example.com", "SecurePass123!@#");
        var (receiver, receiverClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Receiver", "receiver@example.com", "SecurePass123!@#");

        var listResponse = await senderClient.PostAsJsonAsync("/api/lists", TestData.CreateTodoListDto("Shared List"));
        var list = await listResponse.GetContentAsync<TodoListDto>();

        var inviteResponse = await senderClient.PostAsJsonAsync("/api/invites", new CreateInviteDto(receiver.Id, list.Id));
        var invite = await inviteResponse.GetContentAsync<InviteDto>();

        // Act
        var response = await senderClient.DeleteAsync($"/api/invites/{invite.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify invite is removed from sent invites
        var sentInvitesResponse = await senderClient.GetAsync("/api/invites/sent");
        var sentInvites = await sentInvitesResponse.GetContentAsync<ICollection<InviteDto>>();
        sentInvites.Should().BeEmpty();

        // Verify invite is removed from received invites
        var receivedInvitesResponse = await receiverClient.GetAsync("/api/invites/received");
        var receivedInvites = await receivedInvitesResponse.GetContentAsync<ICollection<InviteDto>>();
        receivedInvites.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteInvite_ReturnsNoContent_WhenReceiverDeletes()
    {
        // Arrange
        var (sender, senderClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Sender", "sender@example.com", "SecurePass123!@#");
        var (receiver, receiverClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Receiver", "receiver@example.com", "SecurePass123!@#");

        var listResponse = await senderClient.PostAsJsonAsync("/api/lists", TestData.CreateTodoListDto("Shared List"));
        var list = await listResponse.GetContentAsync<TodoListDto>();

        var inviteResponse = await senderClient.PostAsJsonAsync("/api/invites", new CreateInviteDto(receiver.Id, list.Id));
        var invite = await inviteResponse.GetContentAsync<InviteDto>();

        // Act
        var response = await receiverClient.DeleteAsync($"/api/invites/{invite.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteInvite_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var userClient = Fixture.Factory.CreateClient();

        // Act
        var response = await userClient.DeleteAsync($"/api/invites/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteInvite_WithNonExistentInvite_ReturnsNotFound()
    {
        // Arrange
        var (user, userClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "User", "user@example.com", "SecurePass123!@#");

        // Act
        var response = await userClient.DeleteAsync($"/api/invites/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteInvite_WhenNotSenderOrReceiver_ReturnsForbidden()
    {
        // Arrange
        var (sender, senderClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Sender", "sender@example.com", "SecurePass123!@#");
        var (receiver, receiverClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Receiver", "receiver@example.com", "SecurePass123!@#");
        var (other, otherClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Other", "other@example.com", "SecurePass123!@#");

        var listResponse = await senderClient.PostAsJsonAsync("/api/lists", TestData.CreateTodoListDto("Shared List"));
        var list = await listResponse.GetContentAsync<TodoListDto>();

        var inviteResponse = await senderClient.PostAsJsonAsync("/api/invites", new CreateInviteDto(receiver.Id, list.Id));
        var invite = await inviteResponse.GetContentAsync<InviteDto>();

        // Act
        var response = await otherClient.DeleteAsync($"/api/invites/{invite.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion
}