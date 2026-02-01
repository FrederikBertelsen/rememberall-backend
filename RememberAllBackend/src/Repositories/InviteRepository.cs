using Microsoft.EntityFrameworkCore;
using RememberAll.src.Data;
using RememberAll.src.Entities;
using RememberAll.src.Repositories.Interfaces;

namespace RememberAll.src.Repositories;

public class InviteRepository(AppDbContext dbContext) : IInviteRepository
{
    public async Task<Invite> CreateInviteAsync(Invite invite) => (await dbContext.Invites.AddAsync(invite)).Entity;

    public async Task<ICollection<Invite>> GetRecievedInvitesByUserId(Guid userId) =>
        await dbContext.Invites
            .Include(invite => invite.InviteSender)
            .AsNoTracking()
            .Where(invite => invite.InviteRecieverId == userId)
            .ToListAsync();

    public async Task<ICollection<Invite>> GetSentInvitesByUserId(Guid userId) =>
        await dbContext.Invites
            .Include(invite => invite.InviteReciever)
            .AsNoTracking()
            .Where(invite => invite.InviteSenderId == userId)
            .ToListAsync();

    public async Task<Invite?> GetInviteByIdAsync(Guid inviteId) =>
        await dbContext.Invites
            .Include(invite => invite.InviteSender)
            .Include(invite => invite.InviteReciever)
            .Include(invite => invite.List)
            .AsNoTracking()
            .FirstOrDefaultAsync(invite => invite.Id == inviteId);

    public void DeleteInvite(Invite invite) => dbContext.Invites.Remove(invite);

    public async Task SaveChangesAsync() => await dbContext.SaveChangesAsync();
}