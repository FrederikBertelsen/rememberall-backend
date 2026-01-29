using Microsoft.EntityFrameworkCore;
using RememberAll.src.Data;
using RememberAll.src.Entities;
using RememberAll.src.Repositories.Interfaces;

namespace RememberAll.src.Repositories;

public class ListAccessRepository(AppDbContext dbContext) : IListAccessRepository
{
    public async Task<ListAccess> CreateListAccessAsync(ListAccess listAccess) =>
        (await dbContext.ListAccesss.AddAsync(listAccess)).Entity;
    
    public async Task<ListAccess?> GetListAccessByIdAsync(Guid listAccessId) =>
        await dbContext.ListAccesss
                .Include(listAccess => listAccess.List)
                .Include(listAccess => listAccess.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(listAccesss => listAccesss.Id == listAccessId);
    public async Task<ICollection<ListAccess>> GetListAccesssByUserIdAsync(Guid userId) =>
        await dbContext.ListAccesss
                .Include(listAccess => listAccess.List)
                .Include(listAccess => listAccess.User)
                .AsNoTracking()
                .Where(listAccesss => listAccesss.UserId == userId)
                .ToListAsync();
    public async Task<ICollection<ListAccess>> GetListAccessByListIdAsync(Guid listId) =>
        await dbContext.ListAccesss
                .Include(listAccess => listAccess.List)
                .Include(listAccess => listAccess.User)
                .AsNoTracking()
                .Where(listAccesss => listAccesss.ListId == listId)
                .ToListAsync();
    public void DeleteListAccess(ListAccess listAccess) => dbContext.ListAccesss.Remove(listAccess);

    public async Task SaveChangesAsync() => await dbContext.SaveChangesAsync();
}