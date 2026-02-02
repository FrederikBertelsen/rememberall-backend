using System.Net.Http.Json;
using RememberAll.src.DTOs;

namespace RememberAllBackend.Tests.Helpers;

public static class AuthenticationHelper
{
    #region Builder Factory Methods

    public static AuthenticationBuilder Authenticate(HttpClient client) => new(client);

    #endregion

    #region Direct API Methods

    public static async Task<AuthenticationResult> RegisterAndLoginAsync(
        HttpClient client,
        string? name = null,
        string? email = null,
        string? password = null)
    {
        var userDto = TestData.CreateUserDto(name, email, password);

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", userDto);
        registerResponse.EnsureSuccessStatusCode();

        var user = await registerResponse.Content.ReadFromJsonAsync<UserDto>()
            ?? throw new InvalidOperationException("Failed to deserialize user from registration response");

        var loginDto = TestData.LoginDto(userDto.Email, userDto.Password);
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginDto);
        loginResponse.EnsureSuccessStatusCode();

        return new AuthenticationResult(user, client);
    }

    public static async Task<AuthenticationResult> LoginAsync(
        HttpClient client,
        string email,
        string password)
    {
        var loginDto = new LoginDto(email, password);
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginDto);
        loginResponse.EnsureSuccessStatusCode();

        var user = await loginResponse.Content.ReadFromJsonAsync<UserDto>()
            ?? throw new InvalidOperationException("Failed to deserialize user from login response");

        return new AuthenticationResult(user, client);
    }

    public static async Task<UserDto> GetCurrentUserAsync(HttpClient client)
    {
        var response = await client.GetAsync("/api/auth/me");
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<UserDto>()
            ?? throw new InvalidOperationException("Failed to deserialize current user");
    }

    public static async Task LogoutAsync(HttpClient client)
    {
        var response = await client.PostAsync("/api/auth/logout", null);
        response.EnsureSuccessStatusCode();
    }

    #endregion

    #region Builder

    public class AuthenticationBuilder
    {
        private readonly HttpClient _client;
        private string? _name;
        private string? _email;
        private string? _password;
        private bool _register = true;

        public AuthenticationBuilder(HttpClient client)
        {
            _client = client;
        }

        public AuthenticationBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public AuthenticationBuilder WithEmail(string email)
        {
            _email = email;
            return this;
        }

        public AuthenticationBuilder WithPassword(string password)
        {
            _password = password;
            return this;
        }

        public AuthenticationBuilder WithCredentials(string email, string password)
        {
            _email = email;
            _password = password;
            return this;
        }

        public AuthenticationBuilder AsExistingUser()
        {
            _register = false;
            return this;
        }

        public AuthenticationBuilder AsNewUser()
        {
            _register = true;
            return this;
        }

        public async Task<AuthenticationResult> BuildAsync()
        {
            if (_register)
            {
                return await RegisterAndLoginAsync(_client, _name, _email, _password);
            }

            if (string.IsNullOrEmpty(_email) || string.IsNullOrEmpty(_password))
            {
                throw new InvalidOperationException(
                    "Email and password are required for existing user authentication");
            }

            return await LoginAsync(_client, _email, _password);
        }

        public async Task<List<AuthenticationResult>> BuildMultipleAsync(
            int count,
            string namePrefix = "User",
            string emailPrefix = "user")
        {
            var results = new List<AuthenticationResult>();

            for (int i = 0; i < count; i++)
            {
                var name = $"{namePrefix} {i}";
                var email = $"{emailPrefix}{i}@example.com";
                var password = _password ?? TestData.CreateUserDto().Password;

                var result = await RegisterAndLoginAsync(_client, name, email, password);
                results.Add(result);
            }

            return results;
        }
    }

    #endregion
}

public record AuthenticationResult(UserDto User, HttpClient Client);
