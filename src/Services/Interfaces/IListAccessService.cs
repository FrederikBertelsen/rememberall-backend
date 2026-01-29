using RememberAll.src.DTOs;

namespace RememberAll.src.Services.Interfaces;

public interface IListAccessService
{
    public Task<ListAccessDto> CreateListAccessAsync(CreateListAccessDto createListAccessDto);
    public Task<ICollection<ListAccessDto>> GetListAccesssByUserIdAsync(Guid userId);
    public Task<ICollection<ListAccessDto>> GetListAccesssByListIdAsync(Guid listId);
    public Task DeleteListAccessAsync(Guid listAccessId);
}