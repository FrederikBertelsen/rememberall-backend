using Microsoft.EntityFrameworkCore;
using rememberall.src.Data;
using rememberall.src.Entities;
using rememberall.src.Repositories.Interfaces;

namespace rememberall.src.Repositories;

public class InviteRepository(AppDbContext dbContext) : IInviteRepository
{
    public async Task<Invite> CreateInvite(Invite invite) => (await dbContext.Invites.AddAsync(invite)).Entity;

    public async Task<ICollection<Invite>> GetRecievedInvitesByUserId(Guid userId) => 
        await dbContext.Invites
                .Include(invite => invite.InviterSender)
                .AsNoTracking()
                .Where(invite => invite.InviteRecieverId == userId)
                .ToListAsync();

    public async Task<ICollection<Invite>> GetSentInvitesByUserId(Guid userId) =>
        await dbContext.Invites
                .Include(invite => invite.InviteReciever)
                .AsNoTracking()
                .Where(invite => invite.InviteSenderId == userId)
                .ToListAsync();
    public void DeleteInvite(Invite invite) => dbContext.Invites.Remove(invite);

    public async Task SaveChangesAsync() => await dbContext.SaveChangesAsync();
}