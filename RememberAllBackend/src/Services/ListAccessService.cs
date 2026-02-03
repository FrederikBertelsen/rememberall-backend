using RememberAll.src.DTOs;
using RememberAll.src.Entities;
using RememberAll.src.Exceptions;
using RememberAll.src.Extensions;
using RememberAll.src.Repositories.Interfaces;
using RememberAll.src.Services.Interfaces;

namespace RememberAll.src.Services;

public class ListAccessService(
    IListAccessRepository listAccessRepository,
    ICurrentUserService currentUserService
    ) : IListAccessService
{

    public async Task<ICollection<ListAccessDto>> GetListAccesssByUserAsync()
    {
        Guid userId = currentUserService.GetUserId();

        ICollection<ListAccess> listAccess = await listAccessRepository.GetListAccesssByUserIdAsync(userId)
            ?? throw new NotFoundException("ListAccess", "UserId", userId);

        return listAccess.ToDtos();
    }

    public async Task<ICollection<ListAccessDto>> GetListAccesssByListIdAsync(Guid listId)
    {
        if (listId == Guid.Empty)
            throw new MissingValueException("List Id");

        ICollection<ListAccess> listAccess = await listAccessRepository.GetListAccessByListIdAsync(listId)
            ?? throw new NotFoundException("ListAccess", "ListId", listId);

        if (!await listAccessRepository.UserHasAccessToListAsync(currentUserService.GetUserId(), listId))
            throw new ForbiddenException("User Doesn't have access to todo list.");

        return listAccess.ToDtos();
    }

    public async Task DeleteListAccessAsync(Guid listAccessId)
    {
        if (listAccessId == Guid.Empty)
            throw new MissingValueException("ListAccess Id");

        ListAccess listAccess = await listAccessRepository.GetListAccessByIdAsync(listAccessId)
            ?? throw new NotFoundException("ListAccess", "Id", listAccessId);

        var userId = currentUserService.GetUserId();

        if (listAccess.List!.OwnerId != userId && listAccess.UserId != userId)
            throw new ForbiddenException("User is neither the owner of the todo list nor the user of the list access.");

        listAccessRepository.DeleteListAccess(listAccess);

        await listAccessRepository.SaveChangesAsync();
    }
}