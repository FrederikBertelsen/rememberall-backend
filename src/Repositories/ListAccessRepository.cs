using Microsoft.EntityFrameworkCore;
using RememberAll.src.Data;
using RememberAll.src.Entities;
using RememberAll.src.Repositories.Interfaces;

namespace RememberAll.src.Repositories;

public class ListAccessRepository(AppDbContext dbContext) : IListAccessRepository
{
    public async Task<ListAccess> CreateListAccessAsync(ListAccess listAccess) =>
        (await dbContext.ListAccess.AddAsync(listAccess)).Entity;

    public async Task<ListAccess?> GetListAccessByIdAsync(Guid listAccessId) =>
        await dbContext.ListAccess
            .Include(listAccess => listAccess.List)
            .Include(listAccess => listAccess.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(listAccesss => listAccesss.Id == listAccessId);
    public async Task<ICollection<ListAccess>> GetListAccesssByUserIdAsync(Guid userId) =>
        await dbContext.ListAccess
            .Include(listAccess => listAccess.List)
            .Include(listAccess => listAccess.User)
            .AsNoTracking()
            .Where(listAccesss => listAccesss.UserId == userId)
            .ToListAsync();
    public async Task<ICollection<ListAccess>> GetListAccessByListIdAsync(Guid listId) =>
        await dbContext.ListAccess
            .Include(listAccess => listAccess.List)
            .Include(listAccess => listAccess.User)
            .AsNoTracking()
            .Where(listAccesss => listAccesss.ListId == listId)
            .ToListAsync();

    public async Task<bool> UserHasAccessToListAsync(Guid userId, Guid listId)
    {
        return
            // Check if user is owner of the list
            await dbContext.TodoLists
                .AsNoTracking()
                .AnyAsync(todoList => todoList.Id == listId && todoList.OwnerId == userId)
            // Or has been granted access
            || await dbContext.ListAccess
                .AsNoTracking()
                .AnyAsync(listAccesss => listAccesss.ListId == listId && listAccesss.UserId == userId);
    }

    public void DeleteListAccess(ListAccess listAccess) => dbContext.ListAccess.Remove(listAccess);

    public async Task SaveChangesAsync() => await dbContext.SaveChangesAsync();
}