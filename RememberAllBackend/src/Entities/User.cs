using RememberAll.src.Exceptions;

namespace RememberAll.src.Entities;

public class User : BaseEntity
{
    public required string Name { get; init; }
    public required string Email { get; init; }
    public string PasswordHash { get; private set; } = "";
    public ICollection<TodoList> Lists { get; set; } = [];
    public ICollection<ListAccess> ListAccess { get; set; } = [];
    public ICollection<Invite> InvitesSent { get; set; } = [];
    public ICollection<Invite> InvitesReceived { get; set; } = [];

    public void SetPasswordHash(string passwordHash)
    {
        if (!string.IsNullOrEmpty(PasswordHash))
            throw new BusinessLogicException("PasswordHash already set");

        PasswordHash = passwordHash;
    }
}