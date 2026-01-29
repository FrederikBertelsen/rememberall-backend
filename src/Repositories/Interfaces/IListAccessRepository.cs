using RememberAll.src.Entities;

namespace RememberAll.src.Repositories.Interfaces;

public interface IListAccessRepository
{
    public Task<ListAccess> CreateListAccessAsync(ListAccess listAccess);
    public Task<ListAccess?> GetListAccessByIdAsync(Guid listAccessId);
    public Task<ICollection<ListAccess>> GetListAccesssByUserIdAsync(Guid userId);
    public Task<ICollection<ListAccess>> GetListAccessByListIdAsync(Guid listId);
    public void DeleteListAccess(ListAccess listAccess);

    public Task SaveChangesAsync();
}