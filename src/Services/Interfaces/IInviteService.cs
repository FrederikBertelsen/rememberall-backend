using RememberAll.src.DTOs;

namespace RememberAll.src.Services.Interfaces;

public interface IInviteService
{
    public Task<InviteDto> CreateInvite(CreateInviteDto createInviteDto);
    public Task<ICollection<InviteDto>> GetRecievedInvitesByUserId(Guid userId);
    public Task<ICollection<InviteDto>> GetSentInvitesByUserId(Guid userId);
    public Task AcceptInviteById(Guid inviteId);
    public Task DeleteInviteById(Guid inviteId);
}