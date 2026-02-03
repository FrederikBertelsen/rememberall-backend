using FluentAssertions;
using RememberAll.src.DTOs;
using RememberAll.src.DTOs.Create;
using RememberAllBackend.Tests.Base;
using RememberAllBackend.Tests.Fixtures;
using RememberAllBackend.Tests.Helpers;
using System.Net;

namespace RememberAllBackend.Tests.Integration.Api;

[Trait("Category", "Integration")]
public class AuthApiTests(IndividualDatabaseFixture fixture) : BaseIntegrationTest(fixture)
{
    #region Register Tests

    [Fact]
    public async Task Register_WithValidData_ReturnsUserDto()
    {
        // Arrange
        var userClient = Fixture.Factory.CreateClient();
        var createUserDto = TestData.CreateUserDto("Alice", "alice@example.com", "SecurePass123!@#");

        // Act
        var response = await userClient.PostAsJsonAsync("/api/auth/register", createUserDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.GetContentAsync<UserDto>();
        user.Should().NotBeNull();
        user.Name.Should().Be("Alice");
        user.Email.Should().Be("alice@example.com");
        user.Id.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Register_WithMissingName_ReturnsBadRequest(string invalidName)
    {
        // Arrange
        var userClient = Fixture.Factory.CreateClient();
        var createUserDto = new CreateUserDto(invalidName, "alice@example.com", "SecurePass123!@#");

        // Act
        var response = await userClient.PostAsJsonAsync("/api/auth/register", createUserDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Register_WithMissingEmail_ReturnsBadRequest(string invalidEmail)
    {
        // Arrange
        var userClient = Fixture.Factory.CreateClient();
        var createUserDto = new CreateUserDto("Alice", invalidEmail, "SecurePass123!@#");

        // Act
        var response = await userClient.PostAsJsonAsync("/api/auth/register", createUserDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Register_WithMissingPassword_ReturnsBadRequest(string invalidPassword)
    {
        // Arrange
        var userClient = Fixture.Factory.CreateClient();
        var createUserDto = new CreateUserDto("Alice", "alice@example.com", invalidPassword);

        // Act
        var response = await userClient.PostAsJsonAsync("/api/auth/register", createUserDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("weak")]
    [InlineData("123456")]
    public async Task Register_WithWeakPassword_ReturnsBadRequest(string weakPassword)
    {
        // Arrange
        var userClient = Fixture.Factory.CreateClient();
        var createUserDto = new CreateUserDto("Alice", "alice@example.com", weakPassword);

        // Act
        var response = await userClient.PostAsJsonAsync("/api/auth/register", createUserDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsUserDtoAndSetsCookie()
    {
        // Arrange
        var userClient = Fixture.Factory.CreateClient();
        var createUserDto = TestData.CreateUserDto("Alice", "alice@example.com", "SecurePass123!@#");
        await userClient.PostAsJsonAsync("/api/auth/register", createUserDto);

        var loginDto = TestData.LoginDto("alice@example.com", "SecurePass123!@#");

        // Act
        var response = await userClient.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.GetContentAsync<UserDto>();
        user.Name.Should().Be("Alice");
        user.Email.Should().Be("alice@example.com");

        // Verify authentication cookie is set
        response.Headers.TryGetValues("Set-Cookie", out var cookies).Should().BeTrue();
        cookies.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var userClient = Fixture.Factory.CreateClient();
        var createUserDto = TestData.CreateUserDto("Alice", "alice@example.com", "SecurePass123!@#");
        await userClient.PostAsJsonAsync("/api/auth/register", createUserDto);

        var loginDto = TestData.LoginDto("alice@example.com", "WrongPassword123!@#");

        // Act
        var response = await userClient.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ReturnsUnauthorized()
    {
        // Arrange
        var userClient = Fixture.Factory.CreateClient();
        var loginDto = TestData.LoginDto("nonexistent@example.com", "SecurePass123!@#");

        // Act
        var response = await userClient.PostAsJsonAsync("/api/auth/login", loginDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Logout Tests

    [Fact]
    public async Task Logout_WhenAuthenticated_ReturnsNoContent()
    {
        // Arrange
        var (user, userClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(
            Fixture, "Alice", "alice@example.com", "SecurePass123!@#");

        // Act
        var response = await userClient.PostAsync("/api/auth/logout", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Logout_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var userClient = Fixture.Factory.CreateClient();

        // Act
        var response = await userClient.PostAsync("/api/auth/logout", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Delete Account Tests

    [Fact]
    public async Task DeleteAccount_WhenAuthenticated_RemovesUser()
    {
        // Arrange
        var (user, userClient) = await AuthenticationHelper.CreateRegisterAndLoginAsync(
            Fixture, "Alice", "alice@example.com", "SecurePass123!@#");

        // Act
        var response = await userClient.DeleteAsync("/api/auth/delete-account");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify user can no longer login
        var loginDto = TestData.LoginDto("alice@example.com", "SecurePass123!@#");
        var loginClient = Fixture.Factory.CreateClient();
        var loginResponse = await loginClient.PostAsJsonAsync("/api/auth/login", loginDto);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteAccount_WhenUnauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var userClient = Fixture.Factory.CreateClient();

        // Act
        var response = await userClient.DeleteAsync("/api/auth/delete-account");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Password Requirements Tests

    [Fact]
    public async Task GetPasswordRequirements_ReturnsValidationRules()
    {
        // Arrange
        var userClient = Fixture.Factory.CreateClient();

        // Act
        var response = await userClient.GetAsync("/api/auth/password-requirements");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var requirements = await response.Content.ReadAsStringAsync();
        requirements.Should().NotBeNullOrEmpty();
    }

    #endregion
}
