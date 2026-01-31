using RememberAll.src.DTOs;
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
        User inviteSender = await userRepository.GetUserByIdAsync(currentUserId)
            ?? throw new NotFoundException("User", "Id", currentUserId);

        User inviteReciever = await userRepository.GetUserByIdAsync(createInviteDto.InviteRecieverId)
            ?? throw new NotFoundException("User", "Id", createInviteDto.InviteRecieverId);

        TodoList todoList = await todoListRepository.GetTodoListByIdAsync(createInviteDto.ListId)
            ?? throw new NotFoundException("TodoList", "Id", createInviteDto.ListId);

        Invite newInvite = createInviteDto.ToEntity(inviteSender, inviteReciever, todoList);
        Invite createdInvite = await inviteRepository.CreateInviteAsync(newInvite);

        await inviteRepository.SaveChangesAsync();

        return createdInvite.ToDto();
    }

    public async Task<ICollection<InviteDto>> GetRecievedInvitesByUserIdAsync()
    {
        var userId = currentUserService.GetUserId();

        if (!await userRepository.UserExistsByIdAsync(userId))
            throw new NotFoundException("User", "Id", userId);

        ICollection<Invite> invites = await inviteRepository.GetRecievedInvitesByUserId(userId);

        return invites.ToDtos();
    }

    public async Task<ICollection<InviteDto>> GetSentInvitesByUserIdAsync()
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

        inviteRepository.DeleteInvite(invite);

        await inviteRepository.SaveChangesAsync();
    }
}