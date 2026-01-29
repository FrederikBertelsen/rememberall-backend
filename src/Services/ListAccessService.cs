using RememberAll.src.DTOs;
using RememberAll.src.Entities;
using RememberAll.src.Exceptions;
using RememberAll.src.Extensions;
using RememberAll.src.Repositories.Interfaces;
using RememberAll.src.Services.Interfaces;

namespace RememberAll.src.Services;

public class ListAccessService(IUserRepository userRepository, ITodoListRepository todoListRepository, IListAccessRepository listAccessRepository) : IListAccessService
{
    public async Task<ListAccessDto> CreateListAccessAsync(CreateListAccessDto createListAccessDto)
    {
        createListAccessDto.ValidateOrThrow();

        User user = await userRepository.GetUserByIdAsync(createListAccessDto.UserId)
            ?? throw new NotFoundException("User", "Id", createListAccessDto.UserId);

        TodoList todoList = await todoListRepository.GetTodoListByIdAsync(createListAccessDto.ListId)
            ?? throw new NotFoundException("TodoList", "Id", createListAccessDto.ListId);

        ListAccess newListAccess = createListAccessDto.ToEntity(user, todoList);
        ListAccess createdListAccess = await listAccessRepository.CreateListAccessAsync(newListAccess);

        await listAccessRepository.SaveChangesAsync();

        return createdListAccess.ToDto();
    }

    public async Task<ICollection<ListAccessDto>> GetListAccesssByUserIdAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new MissingValueException("ListAccess", "UserId");

        ICollection<ListAccess> listAccess = await listAccessRepository.GetListAccesssByUserIdAsync(userId)
            ?? throw new NotFoundException("ListAccess", "UserId", userId);

        return listAccess.ToDtos();
    }

    public async Task<ICollection<ListAccessDto>> GetListAccesssByListIdAsync(Guid listId)
    {
        if (listId == Guid.Empty)
            throw new MissingValueException("ListAccess", "ListId");

        ICollection<ListAccess> listAccess = await listAccessRepository.GetListAccessByListIdAsync(listId)
            ?? throw new NotFoundException("ListAccess", "ListId", listId);

        return listAccess.ToDtos();
    }

    public async Task DeleteListAccess(Guid listAccessId)
    {
        if (listAccessId == Guid.Empty)
            throw new MissingValueException("ListAccess", "Id");

        ListAccess listAccess = await listAccessRepository.GetListAccessByIdAsync(listAccessId)
            ?? throw new NotFoundException("ListAccess", "Id", listAccessId);

        listAccessRepository.DeleteListAccess(listAccess);

        await listAccessRepository.SaveChangesAsync();
    }
}