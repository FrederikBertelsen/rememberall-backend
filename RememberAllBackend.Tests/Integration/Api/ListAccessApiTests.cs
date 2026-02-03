using FluentAssertions;
using RememberAll.src.DTOs;
using RememberAll.src.DTOs.Create;
using RememberAllBackend.Tests.Base;
using RememberAllBackend.Tests.Fixtures;
using RememberAllBackend.Tests.Helpers;
using System.Net;

namespace RememberAllBackend.Tests.Integration.Api;

[Trait("Category", "Integration")]
public class ListAccessApiTests(IndividualDatabaseFixture fixture) : BaseIntegrationTest(fixture)
{
    #region Get List Access Tests

    [Fact]
    public async Task GetListAccess_WithoutListId_ReturnsUserAccess()
    {
        // Arrange
        var (user1, user1Client) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "User1", "user1@example.com", "SecurePass123!@#");
        var (user2, user2Client) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "User2", "user2@example.com", "SecurePass123!@#");

        // Create lists owned by different users
        // var list1Response = await user1Client.PostAsJsonAsync("/api/lists", TestData.CreateTodoListDto("User1's List"));
        // var list1 = await list1Response.GetContentAsync<TodoListDto>();

        var list2Response = await user2Client.PostAsJsonAsync("/api/lists", TestData.CreateTodoListDto("User2's List"));
        var list2 = await list2Response.GetContentAsync<TodoListDto>();

        // Create and accept invite so user1 has access to list2
        var inviteResponse = await user2Client.PostAsJsonAsync("/api/invites", new CreateInviteDto(user1.Id, list2.Id));
        var invite = await inviteResponse.GetContentAsync<InviteDto>();
        await user1Client.PatchAsync($"/api/invites/{invite.Id}/accept", null);

        // Act
        var response = await user1Client.GetAsync("/api/listaccess");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var accesses = await response.GetContentAsync<ICollection<ListAccessDto>>();
        accesses.Should().NotBeNull();
        accesses.Should().HaveCount(1);
        accesses.Should().Contain(a => a.ListId == list2.Id && a.UserId == user1.Id);
    }

    [Fact]
    public async Task GetListAccess_WithListId_ReturnsListAccessors()
    {
        // Arrange
        var (owner, ownerClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Owner", "owner@example.com", "SecurePass123!@#");
        var (user1, user1Client) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "User1", "user1@example.com", "SecurePass123!@#");
        var (user2, user2Client) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "User2", "user2@example.com", "SecurePass123!@#");

        var listResponse = await ownerClient.PostAsJsonAsync("/api/lists", TestData.CreateTodoListDto("Shared List"));
        var list = await listResponse.GetContentAsync<TodoListDto>();

        // Create and accept invites for both users
        var invite1Response = await ownerClient.PostAsJsonAsync("/api/invites", new CreateInviteDto(user1.Id, list.Id));
        var invite1 = await invite1Response.GetContentAsync<InviteDto>();
        await user1Client.PatchAsync($"/api/invites/{invite1.Id}/accept", null);

        var invite2Response = await ownerClient.PostAsJsonAsync("/api/invites", new CreateInviteDto(user2.Id, list.Id));
        var invite2 = await invite2Response.GetContentAsync<InviteDto>();
        await user2Client.PatchAsync($"/api/invites/{invite2.Id}/accept", null);

        // Act
        var response = await ownerClient.GetAsync($"/api/listaccess?listId={list.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var accesses = await response.GetContentAsync<ICollection<ListAccessDto>>();
        accesses.Should().NotBeNull();
        accesses.Should().HaveCount(2); // shared users
        accesses.Should().Contain(a => a.ListId == list.Id && a.UserId == user1.Id);
        accesses.Should().Contain(a => a.ListId == list.Id && a.UserId == user2.Id);
    }

    [Fact]
    public async Task GetListAccess_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var userClient = Fixture.Factory.CreateClient();

        // Act
        var response = await userClient.GetAsync("/api/listaccess");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetListAccess_WithNonExistentListId_ReturnsEmptyCollection()
    {
        // Arrange
        var (user, userClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "User", "user@example.com", "SecurePass123!@#");

        // Act
        var response = await userClient.GetAsync($"/api/listaccess?listId={Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetListAccess_WithListUserHasNoAccess_ReturnsEmptyCollection()
    {
        // Arrange
        var (owner, ownerClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Owner", "owner@example.com", "SecurePass123!@#");
        var (other, otherClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Other", "other@example.com", "SecurePass123!@#");

        var listResponse = await ownerClient.PostAsJsonAsync("/api/lists", TestData.CreateTodoListDto("Private List"));
        var list = await listResponse.GetContentAsync<TodoListDto>();

        // Act
        var response = await otherClient.GetAsync($"/api/listaccess?listId={list.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetListAccess_WhenUserHasNoAccess_ReturnsEmptyCollection()
    {
        // Arrange
        var (user, userClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "User", "user@example.com", "SecurePass123!@#");

        // Act
        var response = await userClient.GetAsync("/api/listaccess");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var accesses = await response.GetContentAsync<ICollection<ListAccessDto>>();
        accesses.Should().NotBeNull();
        accesses.Should().BeEmpty();
    }

    #endregion

    #region Delete List Access Tests

    [Fact]
    public async Task DeleteListAccess_ReturnsNoContent_WhenValid()
    {
        // Arrange
        var (owner, ownerClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Owner", "owner@example.com", "SecurePass123!@#");
        var (user, userClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "User", "user@example.com", "SecurePass123!@#");

        var listResponse = await ownerClient.PostAsJsonAsync("/api/lists", TestData.CreateTodoListDto("Shared List"));
        var list = await listResponse.GetContentAsync<TodoListDto>();

        // Create and accept invite
        var inviteResponse = await ownerClient.PostAsJsonAsync("/api/invites", new CreateInviteDto(user.Id, list.Id));
        var invite = await inviteResponse.GetContentAsync<InviteDto>();
        await userClient.PatchAsync($"/api/invites/{invite.Id}/accept", null);

        // Get the list access to delete
        var accessResponse = await ownerClient.GetAsync($"/api/listaccess?listId={list.Id}");
        var accesses = await accessResponse.GetContentAsync<ICollection<ListAccessDto>>();
        var userAccess = accesses.First(a => a.UserId == user.Id);

        // Act
        var response = await ownerClient.DeleteAsync($"/api/listaccess/{userAccess.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify access is removed
        var postDeleteResponse = await ownerClient.GetAsync($"/api/listaccess?listId={list.Id}");
        var postDeleteAccesses = await postDeleteResponse.GetContentAsync<ICollection<ListAccessDto>>();
        postDeleteAccesses.Should().HaveCount(0);

        // Verify user can no longer access the list
        var listAccessResponse = await userClient.GetAsync($"/api/lists/{list.Id}");
        listAccessResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteListAccess_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var userClient = Fixture.Factory.CreateClient();

        // Act
        var response = await userClient.DeleteAsync($"/api/listaccess/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteListAccess_WithNonExistentAccess_ReturnsNotFound()
    {
        // Arrange
        var (user, userClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "User", "user@example.com", "SecurePass123!@#");

        // Act
        var response = await userClient.DeleteAsync($"/api/listaccess/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteListAccess_WhenUserHasNoAccess_ReturnsForbidden()
    {
        // Arrange
        var (owner, ownerClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Owner", "owner@example.com", "SecurePass123!@#");
        var (user1, user1Client) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "User1", "user1@example.com", "SecurePass123!@#");
        var (user2, user2Client) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "User2", "user2@example.com", "SecurePass123!@#");

        var listResponse = await ownerClient.PostAsJsonAsync("/api/lists", TestData.CreateTodoListDto("Shared List"));
        var list = await listResponse.GetContentAsync<TodoListDto>();

        // Create and accept invite for user1
        var inviteResponse = await ownerClient.PostAsJsonAsync("/api/invites", new CreateInviteDto(user1.Id, list.Id));
        var invite = await inviteResponse.GetContentAsync<InviteDto>();
        await user1Client.PatchAsync($"/api/invites/{invite.Id}/accept", null);

        // Get user1's list access
        var accessResponse = await ownerClient.GetAsync($"/api/listaccess?listId={list.Id}");
        var accesses = await accessResponse.GetContentAsync<ICollection<ListAccessDto>>();
        var user1Access = accesses.First(a => a.UserId == user1.Id);

        // Act - user2 tries to delete user1's access
        var response = await user2Client.DeleteAsync($"/api/listaccess/{user1Access.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteListAccess_UserCanDeleteOwnAccess()
    {
        // Arrange
        var (owner, ownerClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Owner", "owner@example.com", "SecurePass123!@#");
        var (user, userClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "User", "user@example.com", "SecurePass123!@#");

        var listResponse = await ownerClient.PostAsJsonAsync("/api/lists", TestData.CreateTodoListDto("Shared List"));
        var list = await listResponse.GetContentAsync<TodoListDto>();

        // Create and accept invite
        var inviteResponse = await ownerClient.PostAsJsonAsync("/api/invites", new CreateInviteDto(user.Id, list.Id));
        var invite = await inviteResponse.GetContentAsync<InviteDto>();
        await userClient.PatchAsync($"/api/invites/{invite.Id}/accept", null);

        // Get user's own list access
        var userAccessResponse = await userClient.GetAsync("/api/listaccess");
        var userAccesses = await userAccessResponse.GetContentAsync<ICollection<ListAccessDto>>();
        var userAccess = userAccesses.First(a => a.ListId == list.Id);

        // Act - user deletes own access
        var response = await userClient.DeleteAsync($"/api/listaccess/{userAccess.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify user can no longer access the list
        var listAccessResponse = await userClient.GetAsync($"/api/lists/{list.Id}");
        listAccessResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion
}