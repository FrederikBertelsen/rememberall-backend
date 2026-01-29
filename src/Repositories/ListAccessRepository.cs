using Microsoft.EntityFrameworkCore;
using rememberall.src.Data;
using rememberall.src.Entities;
using rememberall.src.Repositories.Interfaces;

namespace rememberall.src.Repositories;

public class ListAccessRepository(AppDbContext dbContext) : IListAccessRepository
{
    public async Task<ListAccess> CreateListAccessAsync(ListAccess listAccess) =>
        (await dbContext.ListAccesss.AddAsync(listAccess)).Entity;
    public async Task<ICollection<ListAccess>> GetListAccesssByUserIdAsync(Guid userId) =>
        await dbContext.ListAccesss
                .AsNoTracking()
                .Where(listAccesss => listAccesss.UserId == userId)
                .ToListAsync();
    public async Task<ICollection<ListAccess>> GetListAccesssByListIdAsync(Guid listId) =>
        await dbContext.ListAccesss
                .AsNoTracking()
                .Where(listAccesss => listAccesss.ListId == listId)
                .ToListAsync();
    public void DeleteListAccess(ListAccess listAccess) => dbContext.ListAccesss.Remove(listAccess);

    public async Task SaveChangesAsync() => await dbContext.SaveChangesAsync();
}