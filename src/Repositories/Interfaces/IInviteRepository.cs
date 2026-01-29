using RememberAll.src.Entities;

namespace RememberAll.src.Repositories.Interfaces;

public interface IInviteRepository
{
    public Task<Invite> CreateInvite(Invite invite);
    public Task<ICollection<Invite>> GetRecievedInvitesByUserId(Guid userId);
    public Task<ICollection<Invite>> GetSentInvitesByUserId(Guid userId);
    public void DeleteInvite(Invite invite);

    public Task SaveChangesAsync();
}