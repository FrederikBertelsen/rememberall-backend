using System.Security.Claims;
using RememberAll.src.Entities;
using RememberAll.src.Exceptions;
using RememberAll.src.Services.Interfaces;

namespace RememberAll.src.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid GetUserId()
    {
        var idClaim = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(idClaim, out var userId)
            ? userId
            : throw new AuthException("Authentication failed", "User ID claim is missing or invalid.");
    }

    public string GetUserEmail() =>
        httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email)
        ?? throw new AuthException("Authentication failed", "User Email claim is missing.");

    public bool IsCurrentUser(User user) => IsCurrentUser(user.Id) && GetUserEmail() == user.Email;
    public bool IsCurrentUser(Guid userId) => GetUserId() == userId;
    public bool IsCurrentUserByEmail(string email) => GetUserEmail() == email;
}