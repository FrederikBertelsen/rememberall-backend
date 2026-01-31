using System.Security.Claims;
using RememberAll.src.Entities;
using RememberAll.src.Exceptions;
using RememberAll.src.Services.Interfaces;

namespace RememberAll.src.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public Guid? GetUserId()
    {
        var idClaim = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(idClaim, out var userId)
            ? userId
            : null;
    }

    public string? GetUserEmail() =>
        httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email)
        ?? null;

    public bool IsCurrentUser(User user) => IsCurrentUser(user.Id) && GetUserEmail() == user.Email;
    public bool IsCurrentUser(Guid userId) => GetUserId() == userId;
    public bool IsCurrentUserByEmail(string email) => GetUserEmail() == email;
}