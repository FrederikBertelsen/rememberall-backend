using FluentAssertions;
using RememberAll.src.DTOs;
using RememberAll.src.DTOs.Update;
using RememberAllBackend.Tests.Base;
using RememberAllBackend.Tests.Fixtures;
using RememberAllBackend.Tests.Helpers;
using System.Net;

namespace RememberAllBackend.Tests.Integration.Api;

[Trait("Category", "Integration")]
public class TodoItemApiTests(IndividualDatabaseFixture fixture) : BaseIntegrationTest(fixture)
{
    #region Create TodoItem Tests

    [Fact]
    public async Task CreateTodoItem_ReturnsDto_WhenValid()
    {
        // Arrange
        var (user, userClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Alice", "alice@example.com", "SecurePass123!@#");

        var listResponse = await userClient.PostAsJsonAsync("/api/lists", TestData.CreateTodoListDto("Shopping List"));
        var list = await listResponse.GetContentAsync<TodoListDto>();

        var createDto = TestData.CreateTodoItemDto(list.Id, "Buy milk");

        // Act
        var response = await userClient.PostAsJsonAsync("/api/todoitems", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var item = await response.GetContentAsync<TodoItemDto>();
        item.Should().NotBeNull();
        item.Text.Should().Be("Buy milk");
        item.IsCompleted.Should().BeFalse();
        item.CompletionCount.Should().Be(0);
    }

    [Fact]
    public async Task CreateTodoItem_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var createDto = TestData.CreateTodoItemDto(Guid.NewGuid(), "Buy milk");

        // Act
        var response = await Client.PostAsJsonAsync("/api/todoitems", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateTodoItem_WithInvalidText_ReturnsBadRequest(string invalidText)
    {
        // Arrange
        var (user, userClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Alice", "alice@example.com", "SecurePass123!@#");

        var listResponse = await userClient.PostAsJsonAsync("/api/lists", TestData.CreateTodoListDto("Shopping List"));
        var list = await listResponse.GetContentAsync<TodoListDto>();

        var createDto = TestData.CreateTodoItemDto(list.Id, invalidText);

        // Act
        var response = await userClient.PostAsJsonAsync("/api/todoitems", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTodoItem_WithNonExistentList_ReturnsNotFound()
    {
        // Arrange
        var (user, userClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Alice", "alice@example.com", "SecurePass123!@#");

        var createDto = TestData.CreateTodoItemDto(Guid.NewGuid(), "Buy milk");

        // Act
        var response = await userClient.PostAsJsonAsync("/api/todoitems", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateTodoItem_WhenUserHasNoAccess_ReturnsForbidden()
    {
        // Arrange
        var (owner, ownerClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Owner", "owner@example.com", "SecurePass123!@#");
        var (other, otherClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Other", "other@example.com", "SecurePass123!@#");

        var listResponse = await ownerClient.PostAsJsonAsync("/api/lists", TestData.CreateTodoListDto("Private List"));
        var list = await listResponse.GetContentAsync<TodoListDto>();

        var createDto = TestData.CreateTodoItemDto(list.Id, "Buy milk");

        // Act
        var response = await otherClient.PostAsJsonAsync("/api/todoitems", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Get TodoItems by List Tests

    [Fact]
    public async Task GetTodoItemsByListId_ReturnsItems_WhenUserHasAccess()
    {
        // Arrange
        var (user, userClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Alice", "alice@example.com", "SecurePass123!@#");

        var listResponse = await userClient.PostAsJsonAsync("/api/lists", TestData.CreateTodoListDto("Shopping List"));
        var list = await listResponse.GetContentAsync<TodoListDto>();

        // Create a few items
        await userClient.PostAsJsonAsync("/api/todoitems", TestData.CreateTodoItemDto(list.Id, "Buy milk"));
        await userClient.PostAsJsonAsync("/api/todoitems", TestData.CreateTodoItemDto(list.Id, "Buy bread"));

        // Act
        var response = await userClient.GetAsync($"/api/todoitems/bylist/{list.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var items = await response.GetContentAsync<ICollection<TodoItemDto>>();
        items.Should().NotBeNull();
        items.Should().HaveCount(2);
        items.Should().Contain(i => i.Text == "Buy milk");
        items.Should().Contain(i => i.Text == "Buy bread");
    }

    [Fact]
    public async Task GetTodoItemsByListId_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.GetAsync($"/api/todoitems/bylist/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTodoItemsByListId_WhenUserHasNoAccess_ReturnsForbidden()
    {
        // Arrange
        var (owner, ownerClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Owner", "owner@example.com", "SecurePass123!@#");
        var (other, otherClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Other", "other@example.com", "SecurePass123!@#");

        var listResponse = await ownerClient.PostAsJsonAsync("/api/lists", TestData.CreateTodoListDto("Private List"));
        var list = await listResponse.GetContentAsync<TodoListDto>();

        // Act
        var response = await otherClient.GetAsync($"/api/todoitems/bylist/{list.Id}");
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetTodoItemsByListId_WithNonExistentList_ReturnsForbidden()
    {
        // Arrange
        var (user, userClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Alice", "alice@example.com", "SecurePass123!@#");

        // Act
        var response = await userClient.GetAsync($"/api/todoitems/bylist/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Update TodoItem Tests

    [Fact]
    public async Task UpdateTodoItem_ReturnsUpdated_WhenValid()
    {
        // Arrange
        var (user, userClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Alice", "alice@example.com", "SecurePass123!@#");

        var listResponse = await userClient.PostAsJsonAsync("/api/lists", TestData.CreateTodoListDto("Shopping List"));
        var list = await listResponse.GetContentAsync<TodoListDto>();

        var createResponse = await userClient.PostAsJsonAsync("/api/todoitems", TestData.CreateTodoItemDto(list.Id, "Buy milk"));
        var item = await createResponse.GetContentAsync<TodoItemDto>();

        var updateDto = new UpdateTodoItemDto(item.Id, "Buy organic milk");

        // Act
        var response = await userClient.PatchAsJsonAsync("/api/todoitems", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedItem = await response.GetContentAsync<TodoItemDto>();
        updatedItem.Should().NotBeNull();
        updatedItem.Id.Should().Be(item.Id);
        updatedItem.Text.Should().Be("Buy organic milk");
    }

    [Fact]
    public async Task UpdateTodoItem_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var updateDto = new UpdateTodoItemDto(Guid.NewGuid(), "Updated text");

        // Act
        var response = await Client.PatchAsJsonAsync("/api/todoitems", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpdateTodoItem_WithInvalidText_ReturnsBadRequest(string invalidText)
    {
        // Arrange
        var (user, userClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Alice", "alice@example.com", "SecurePass123!@#");

        var updateDto = new UpdateTodoItemDto(Guid.NewGuid(), invalidText);

        // Act
        var response = await userClient.PatchAsJsonAsync("/api/todoitems", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateTodoItem_WithNonExistentItem_ReturnsNotFound()
    {
        // Arrange
        var (user, userClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Alice", "alice@example.com", "SecurePass123!@#");

        var updateDto = new UpdateTodoItemDto(Guid.NewGuid(), "Updated text");

        // Act
        var response = await userClient.PatchAsJsonAsync("/api/todoitems", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateTodoItem_WhenUserHasNoAccess_ReturnsForbidden()
    {
        // Arrange
        var (owner, ownerClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Owner", "owner@example.com", "SecurePass123!@#");
        var (other, otherClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Other", "other@example.com", "SecurePass123!@#");

        var listResponse = await ownerClient.PostAsJsonAsync("/api/lists", TestData.CreateTodoListDto("Private List"));
        var list = await listResponse.GetContentAsync<TodoListDto>();

        var createResponse = await ownerClient.PostAsJsonAsync("/api/todoitems", TestData.CreateTodoItemDto(list.Id, "Original text"));
        var item = await createResponse.GetContentAsync<TodoItemDto>();

        var updateDto = new UpdateTodoItemDto(item.Id, "Updated text");

        // Act
        var response = await otherClient.PatchAsJsonAsync("/api/todoitems", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Complete/Incomplete TodoItem Tests

    [Fact]
    public async Task MarkTodoItemAsComplete_ReturnsCompleted_WhenValid()
    {
        // Arrange
        var (user, userClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Alice", "alice@example.com", "SecurePass123!@#");

        var listResponse = await userClient.PostAsJsonAsync("/api/lists", TestData.CreateTodoListDto("Shopping List"));
        var list = await listResponse.GetContentAsync<TodoListDto>();

        var createResponse = await userClient.PostAsJsonAsync("/api/todoitems", TestData.CreateTodoItemDto(list.Id, "Buy milk"));
        var item = await createResponse.GetContentAsync<TodoItemDto>();

        // Act
        var response = await userClient.PatchAsync($"/api/todoitems/{item.Id}/complete", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var completedItem = await response.GetContentAsync<TodoItemDto>();
        completedItem.Should().NotBeNull();
        completedItem.Id.Should().Be(item.Id);
        completedItem.IsCompleted.Should().BeTrue();
        completedItem.CompletionCount.Should().Be(1);
    }

    [Fact]
    public async Task MarkTodoItemAsIncomplete_ReturnsIncomplete_WhenValid()
    {
        // Arrange
        var (user, userClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Alice", "alice@example.com", "SecurePass123!@#");

        var listResponse = await userClient.PostAsJsonAsync("/api/lists", TestData.CreateTodoListDto("Shopping List"));
        var list = await listResponse.GetContentAsync<TodoListDto>();

        var createResponse = await userClient.PostAsJsonAsync("/api/todoitems", TestData.CreateTodoItemDto(list.Id, "Buy milk"));
        var item = await createResponse.GetContentAsync<TodoItemDto>();

        // Mark as complete first
        await userClient.PatchAsync($"/api/todoitems/{item.Id}/complete", null);

        // Act
        var response = await userClient.PatchAsync($"/api/todoitems/{item.Id}/incomplete", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var incompleteItem = await response.GetContentAsync<TodoItemDto>();
        incompleteItem.Should().NotBeNull();
        incompleteItem.Id.Should().Be(item.Id);
        incompleteItem.IsCompleted.Should().BeFalse();
        incompleteItem.CompletionCount.Should().Be(1); // Should preserve completion count
    }

    [Fact]
    public async Task MarkTodoItemAsComplete_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.PatchAsync($"/api/todoitems/{Guid.NewGuid()}/complete", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task MarkTodoItemAsComplete_WithNonExistentItem_ReturnsNotFound()
    {
        // Arrange
        var (user, userClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Alice", "alice@example.com", "SecurePass123!@#");

        // Act
        var response = await userClient.PatchAsync($"/api/todoitems/{Guid.NewGuid()}/complete", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MarkTodoItemAsComplete_WhenUserHasNoAccess_ReturnsForbidden()
    {
        // Arrange
        var (owner, ownerClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Owner", "owner@example.com", "SecurePass123!@#");
        var (other, otherClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Other", "other@example.com", "SecurePass123!@#");

        var listResponse = await ownerClient.PostAsJsonAsync("/api/lists", TestData.CreateTodoListDto("Private List"));
        var list = await listResponse.GetContentAsync<TodoListDto>();

        var createResponse = await ownerClient.PostAsJsonAsync("/api/todoitems", TestData.CreateTodoItemDto(list.Id, "Buy milk"));
        var item = await createResponse.GetContentAsync<TodoItemDto>();

        // Act
        var response = await otherClient.PatchAsync($"/api/todoitems/{item.Id}/complete", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region Delete TodoItem Tests

    [Fact]
    public async Task DeleteTodoItem_ReturnsNoContent_WhenValid()
    {
        // Arrange
        var (user, userClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Alice", "alice@example.com", "SecurePass123!@#");

        var listResponse = await userClient.PostAsJsonAsync("/api/lists", TestData.CreateTodoListDto("Shopping List"));
        var list = await listResponse.GetContentAsync<TodoListDto>();

        var createResponse = await userClient.PostAsJsonAsync("/api/todoitems", TestData.CreateTodoItemDto(list.Id, "Buy milk"));
        var item = await createResponse.GetContentAsync<TodoItemDto>();

        // Act
        var response = await userClient.DeleteAsync($"/api/todoitems/{item.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion - getting list items should not include the deleted item
        var itemsResponse = await userClient.GetAsync($"/api/todoitems/bylist/{list.Id}");
        var items = await itemsResponse.GetContentAsync<ICollection<TodoItemDto>>();
        items.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteTodoItem_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Act
        var response = await Client.DeleteAsync($"/api/todoitems/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteTodoItem_WithNonExistentItem_ReturnsNotFound()
    {
        // Arrange
        var (user, userClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Alice", "alice@example.com", "SecurePass123!@#");

        // Act
        var response = await userClient.DeleteAsync($"/api/todoitems/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTodoItem_WhenUserHasNoAccess_ReturnsForbidden()
    {
        // Arrange
        var (owner, ownerClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Owner", "owner@example.com", "SecurePass123!@#");
        var (other, otherClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Other", "other@example.com", "SecurePass123!@#");

        var listResponse = await ownerClient.PostAsJsonAsync("/api/lists", TestData.CreateTodoListDto("Private List"));
        var list = await listResponse.GetContentAsync<TodoListDto>();

        var createResponse = await ownerClient.PostAsJsonAsync("/api/todoitems", TestData.CreateTodoItemDto(list.Id, "Buy milk"));
        var item = await createResponse.GetContentAsync<TodoItemDto>();

        // Act
        var response = await otherClient.DeleteAsync($"/api/todoitems/{item.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion
}