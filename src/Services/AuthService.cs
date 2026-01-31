using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RememberAll.src.DTOs;
using RememberAll.src.Entities;
using RememberAll.src.Exceptions;
using RememberAll.src.Extensions;
using RememberAll.src.Repositories.Interfaces;
using RememberAll.src.Services.Interfaces;
using RememberAll.src.Utilities;

namespace RememberAll.src.Services;

public class AuthService(IUserRepository userRepository, IPasswordHasher<User> passwordHasher, IHttpContextAccessor httpContextAccessor) : IAuthService
{
    public async Task<UserDto> Register(CreateUserDto createUserDto)
    {
        createUserDto.ValidateOrThrow();

        if (await userRepository.UserExistsByEmailAsync(createUserDto.Email))
            throw new AlreadyExistsException("User", "Email", createUserDto.Email);

        User newUser = createUserDto.ToEntity();

        string passwordHash = passwordHasher.HashPassword(newUser, createUserDto.Password);
        newUser.SetPasswordHash(passwordHash);

        User createdUser = await userRepository.CreateUserAsync(newUser);

        await userRepository.SaveChangesAsync();

        return createdUser.ToDto();

    }
    public async Task<UserDto> Login(LoginDto loginDto)
    {
        User user = await userRepository.GetUserByEmailAsync(loginDto.Email)
            ?? throw new AuthException(new NotFoundException("User", "Email", loginDto.Email).Message);

        var verificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginDto.Password);
        if (verificationResult == PasswordVerificationResult.Failed)
            throw new AuthException("Password verification failed");

        List<Claim> claims =
        [
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.Name)
        ];

        ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        ClaimsPrincipal principal = new(identity);

        await GetHttpContext().SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMonths(6)
            }
        );

        return user.ToDto();
    }

    public async Task Logout() =>
        await GetHttpContext().SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    public async Task<UserDto> Me()
    {
        var idClaim = GetClaimsPrincipal().FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idClaim, out var userId))
            throw new AuthException("Invalid user ID claim");

        var user = await userRepository.GetUserByIdAsync(userId)
            ?? throw new AuthException(new NotFoundException("User", "Id", userId.ToString()).Message);

        return user.ToDto();
    }

    public string GetPasswordRequirements() => PasswordValidator.GetRequirementsMessage();

    private HttpContext GetHttpContext()
    {
        var httpContext = httpContextAccessor.HttpContext
            ?? throw new AuthException("No HttpContext available");

        return httpContext;
    }

    private ClaimsPrincipal GetClaimsPrincipal()
    {
        var claimsPrincipal = GetHttpContext().User;

        if (claimsPrincipal.Identity is null || !claimsPrincipal.Identity.IsAuthenticated)
            throw new AuthException("User is not authenticated");

        return claimsPrincipal;
    }


}