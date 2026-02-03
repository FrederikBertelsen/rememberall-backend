using RememberAll.src.DTOs;
using RememberAll.src.DTOs.Create;
using RememberAll.src.Entities;
using RememberAll.src.Exceptions;
using RememberAll.src.Extensions;
using RememberAll.src.Repositories.Interfaces;
using RememberAll.src.Services.Interfaces;

namespace RememberAll.src.Services;

public class InviteService(
    IUserRepository userRepository,
    ITodoListRepository todoListRepository,
    IInviteRepository inviteRepository,
    IListAccessRepository listAccessRepository,
    ICurrentUserService currentUserService) : IInviteService
{
    public async Task<InviteDto> CreateInviteAsync(CreateInviteDto createInviteDto)
    {
        createInviteDto.ValidateOrThrow();

        var currentUserId = currentUserService.GetUserId();
        if (!await userRepository.UserExistsByIdAsync(currentUserId))
            throw new NotFoundException("User", "Id", currentUserId);

        if (!await userRepository.UserExistsByIdAsync(createInviteDto.InviteRecieverId))
            throw new NotFoundException("User", "Id", createInviteDto.InviteRecieverId);

        TodoList todoList = await todoListRepository.GetTodoListByIdAsync(createInviteDto.ListId)
            ?? throw new NotFoundException("TodoList", "Id", createInviteDto.ListId);

        if (!await listAccessRepository.UserHasAccessToListAsync(currentUserId, todoList.Id))
            throw new ForbiddenException("User does not have access to the specified TodoList.");

        Invite newInvite = createInviteDto.ToEntity(currentUserId);
        newInvite = await inviteRepository.CreateInviteAsync(newInvite);

        await inviteRepository.SaveChangesAsync();

        // Retrieve the invite with all navigation properties loaded
        Invite createdInvite = await inviteRepository.GetInviteByIdAsync(newInvite.Id)
            ?? throw new BusinessLogicException("Failed to retrieve the created invite.");

        return createdInvite.ToDto();
    }

    public async Task<ICollection<InviteDto>> GetReceivedInvitesByUserAsync()
    {
        var userId = currentUserService.GetUserId();

        if (!await userRepository.UserExistsByIdAsync(userId))
            throw new NotFoundException("User", "Id", userId);

        ICollection<Invite> invites = await inviteRepository.GetRecievedInvitesByUserId(userId);

        return invites.ToDtos();
    }

    public async Task<ICollection<InviteDto>> GetSentInvitesByUserAsync()
    {
        var userId = currentUserService.GetUserId();

        if (!await userRepository.UserExistsByIdAsync(userId))
            throw new NotFoundException("User", "Id", userId);

        ICollection<Invite> invites = await inviteRepository.GetSentInvitesByUserId(userId);

        return invites.ToDtos();
    }

    public async Task AcceptInviteByIdAsync(Guid inviteId)
    {
        if (inviteId == Guid.Empty)
            throw new MissingValueException("InviteId");

        Invite invite = await inviteRepository.GetInviteByIdAsync(inviteId)
            ?? throw new NotFoundException("Invite", "Id", inviteId);

        if (invite.List!.OwnerId != invite.InviteSenderId)
            throw new InvalidOperationException("Invite's sender is not the owner of the list.");

        if (!currentUserService.IsCurrentUser(invite.InviteRecieverId))
            throw new ForbiddenException("User cannot accept an invite not addressed to them.");

        ListAccess newListAccess = invite.ToListAccess();
        await listAccessRepository.CreateListAccessAsync(newListAccess);

        inviteRepository.DeleteInvite(invite);

        await inviteRepository.SaveChangesAsync();
    }

    public async Task DeleteInviteByIdAsync(Guid inviteId)
    {
        if (inviteId == Guid.Empty)
            throw new MissingValueException("InviteId");

        Invite invite = await inviteRepository.GetInviteByIdAsync(inviteId)
            ?? throw new NotFoundException("Invite", "Id", inviteId);

        if (!currentUserService.IsCurrentUser(invite.InviteSenderId) &&
            !currentUserService.IsCurrentUser(invite.InviteRecieverId))
            throw new ForbiddenException("User cannot delete an invite not addressed to or sent by them.");

        inviteRepository.DeleteInvite(invite);

        await inviteRepository.SaveChangesAsync();
    }
}