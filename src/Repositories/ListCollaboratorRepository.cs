using Microsoft.EntityFrameworkCore;
using rememberall.src.Data;
using rememberall.src.Entities;
using rememberall.src.Repositories.Interfaces;

namespace rememberall.src.Repositories;

public class ListCollaboratorRepository(AppDbContext dbContext) : IListCollaboratorRepository
{
    public async Task<ListCollaborator> CreateListCollaboratorAsync(ListCollaborator listCollaborator) =>
        (await dbContext.ListCollaborators.AddAsync(listCollaborator)).Entity;
    public async Task<ICollection<ListCollaborator>> GetListCollaboratorsByUserIdAsync(Guid userId) =>
        await dbContext.ListCollaborators
                .AsNoTracking()
                .Where(listCollaborators => listCollaborators.UserId == userId)
                .ToListAsync();
    public async Task<ICollection<ListCollaborator>> GetListCollaboratorsByListIdAsync(Guid listId) =>
        await dbContext.ListCollaborators
                .AsNoTracking()
                .Where(listCollaborators => listCollaborators.ListId == listId)
                .ToListAsync();
    public void DeleteListCollaborator(ListCollaborator listCollaborator) => dbContext.ListCollaborators.Remove(listCollaborator);

    public async Task SaveChangesAsync() => await dbContext.SaveChangesAsync();
}