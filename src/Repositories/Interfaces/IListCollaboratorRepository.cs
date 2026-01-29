using rememberall.src.Entities;

namespace rememberall.src.Repositories.Interfaces;

public interface IListCollaboratorRepository
{
    public Task<ListCollaborator> CreateListCollaboratorAsync(ListCollaborator listCollaborator);
    public Task<ICollection<ListCollaborator>> GetListCollaboratorsByUserIdAsync(Guid userId);
    public Task<ICollection<ListCollaborator>> GetListCollaboratorsByListIdAsync(Guid listId);
    public void DeleteListCollaborator(ListCollaborator listCollaborator);

    public Task SaveChangesAsync();
}