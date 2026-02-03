using RememberAll.src.DTOs;
using RememberAllBackend.Tests.Fixtures;

namespace RememberAllBackend.Tests.Helpers;

public static class AuthenticationHelper
{
    public static async Task<AuthenticationResult> CreateRegisterAndLoginAsync(
        IndividualDatabaseFixture fixture,
        string? name = null,
        string? email = null,
        string? password = null)
    {
        // Create
        var userClient = fixture.Factory.CreateClient();

        // Register
        var userDto = TestData.CreateUserDto(name, email, password);
        var registerResponse = await userClient.PostAsJsonAsync("/api/auth/register", userDto);
        registerResponse.EnsureSuccessStatusCode();

        var user = await registerResponse.Content.ReadFromJsonAsync<UserDto>()
            ?? throw new InvalidOperationException("Failed to deserialize user from registration response");

        // Login
        var loginDto = TestData.LoginDto(userDto.Email, userDto.Password);
        var loginResponse = await userClient.PostAsJsonAsync("/api/auth/login", loginDto);
        loginResponse.EnsureSuccessStatusCode();

        return new AuthenticationResult(user, userClient);
    }

    public static async Task<UserDto> LoginAsync(
        HttpClient userClient,
        string email,
        string password)
    {
        var loginDto = new LoginDto(email, password);
        var loginResponse = await userClient.PostAsJsonAsync("/api/auth/login", loginDto);
        loginResponse.EnsureSuccessStatusCode();

        var userDto = await loginResponse.Content.ReadFromJsonAsync<UserDto>()
            ?? throw new InvalidOperationException("Failed to deserialize user from login response");

        return userDto;
    }

    public static async Task<UserDto> GetCurrentUserAsync(HttpClient userClient)
    {
        var response = await userClient.GetAsync("/api/auth/me");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<UserDto>()
            ?? throw new InvalidOperationException("Failed to deserialize current user");
    }

    public static async Task LogoutAsync(HttpClient userClient)
    {
        var response = await userClient.PostAsync("/api/auth/logout", null);
        response.EnsureSuccessStatusCode();
    }
}

public record AuthenticationResult(UserDto User, HttpClient userClient);
