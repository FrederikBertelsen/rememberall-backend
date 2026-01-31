using RememberAll.src.Entities;

namespace RememberAll.src.Services.Interfaces;

public interface ICurrentUserService
{
    public Guid? GetUserId();
    public string? GetUserEmail();
    bool IsCurrentUser(Guid userId);
    bool IsCurrentUser(User user);
    bool IsCurrentUserByEmail(string email);

}