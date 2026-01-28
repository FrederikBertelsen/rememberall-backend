using rememberall.src.Data;
using rememberall.src.Entities;
using rememberall.src.Repositories.Interfaces;

namespace rememberall.src.Repositories;

public class InviteRepository(AppDbContext dbContext) : IInviteRepository
{
    public async Task<Invite> CreateInvite(Invite invite) => (await dbContext.Invites.AddAsync(invite)).Entity;
    public void DeleteInvite(Invite invite) => dbContext.Invites.Remove(invite);

    public async Task SaveChangesAsync() => await dbContext.SaveChangesAsync();
}