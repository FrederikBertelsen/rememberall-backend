using rememberall.src.Entities;

namespace rememberall.src.Repositories.Interfaces;

public interface IInviteRepository
{
    public Task<Invite> CreateInvite(Invite invite);
    public void DeleteInvite(Invite invite);

    public Task SaveChangesAsync();
}