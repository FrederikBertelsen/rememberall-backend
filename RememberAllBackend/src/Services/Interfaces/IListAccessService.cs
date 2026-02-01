using RememberAll.src.DTOs;

namespace RememberAll.src.Services.Interfaces;

public interface IListAccessService
{
    public Task<ICollection<ListAccessDto>> GetListAccesssByUserAsync();
    public Task<ICollection<ListAccessDto>> GetListAccesssByListIdAsync(Guid listId);
    public Task DeleteListAccessAsync(Guid listAccessId);
}