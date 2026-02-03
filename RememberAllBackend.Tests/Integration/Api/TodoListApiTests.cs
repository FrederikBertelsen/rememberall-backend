using FluentAssertions;
using RememberAll.src.DTOs;
using RememberAll.src.DTOs.Create;
using RememberAllBackend.Tests.Base;
using RememberAllBackend.Tests.Fixtures;
using RememberAllBackend.Tests.Helpers;
using System.Net;

namespace RememberAllBackend.Tests.Integration.Api;

[Trait("Category", "Integration")]
public class TodoListCreateTests(IndividualDatabaseFixture fixture) : BaseIntegrationTest(fixture)
{

    [Fact]
    public async Task CreateList_WithValidName_ReturnsListWithOwner()
    {
        // Arrange
        var (user, userClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "Alice", "alice@example.com", "SecurePass123!@#");

        // Act
        var response = await userClient.PostAsJsonAsync("/api/lists", new CreateTodoListDto("My List"));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await response.GetContentAsync<TodoListDto>();
        list.Name.Should().Be("My List");
        list.OwnerId.Should().Be(user.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateList_WithEmptyName_ReturnsBadRequest(string emptyName)
    {
        // Arrange
        var (user, userClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "bob", "bob@example.com", "SecurePass123!@#");

        // Act
        var response = await userClient.PostAsJsonAsync("/api/lists", new CreateTodoListDto(emptyName));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

[Trait("Category", "Integration")]
public class TodoListReadTests(IndividualDatabaseFixture fixture) : BaseIntegrationTest(fixture)
{

    [Fact]
    public async Task GetListById_WhenOwner_ReturnsListDetails()
    {
        // Arrange
        var (user, userClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "bob", "bob@example.com", "SecurePass123!@#");

        var listResponse = await userClient.PostAsJsonAsync("/api/lists", new CreateTodoListDto("My List"));
        var list = await listResponse.GetContentAsync<TodoListDto>();

        // Act
        var response = await userClient.GetAsync($"/api/lists/{list.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var retrieved = await response.GetContentAsync<TodoListDto>();
        retrieved.Id.Should().Be(list.Id);
        retrieved.Name.Should().Be("My List");
    }

    [Fact]
    public async Task GetListById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var (user, userClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "bob", "bob@example.com", "SecurePass123!@#");

        var fakeId = Guid.NewGuid();

        // Act
        var response = await userClient.GetAsync($"/api/lists/{fakeId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

[Trait("Category", "Integration")]
public class TodoListDeleteTests(IndividualDatabaseFixture fixture) : BaseIntegrationTest(fixture)
{

    [Fact]
    public async Task DeleteList_WhenOwner_RemovesListFromUser()
    {
        // Arrange
        var (user, userClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "bob", "bob@example.com", "SecurePass123!@#");

        var createResponse = await userClient.PostAsJsonAsync("/api/lists", new CreateTodoListDto("To Delete"));
        var list = await createResponse.GetContentAsync<TodoListDto>();

        // Act
        var deleteResponse = await userClient.DeleteAsync($"/api/lists/{list.Id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deletion
        var getResponse = await userClient.GetAsync($"/api/lists/{list.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteList_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var (user, userClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(Fixture, "bob", "bob@example.com", "SecurePass123!@#");

        var fakeId = Guid.NewGuid();

        // Act
        var response = await userClient.DeleteAsync($"/api/lists/{fakeId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
