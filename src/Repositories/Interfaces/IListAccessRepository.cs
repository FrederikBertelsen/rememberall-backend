using rememberall.src.Entities;

namespace rememberall.src.Repositories.Interfaces;

public interface IListAccessRepository
{
    public Task<ListAccess> CreateListAccessAsync(ListAccess listAccess);
    public Task<ICollection<ListAccess>> GetListAccesssByUserIdAsync(Guid userId);
    public Task<ICollection<ListAccess>> GetListAccesssByListIdAsync(Guid listId);
    public void DeleteListAccess(ListAccess listAccess);

    public Task SaveChangesAsync();
}