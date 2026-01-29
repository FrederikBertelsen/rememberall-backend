using RememberAll.src.DTOs;

namespace RememberAll.src.Services.Interfaces;

public interface IInviteService
{
    public Task<InviteDto> CreateInviteAsync(CreateInviteDto createInviteDto);
    public Task<ICollection<InviteDto>> GetRecievedInvitesByUserIdAsync(Guid userId);
    public Task<ICollection<InviteDto>> GetSentInvitesByUserIdAsync(Guid userId);
    public Task AcceptInviteByIdAsync(Guid inviteId);
    public Task DeleteInviteByIdAsync(Guid inviteId);
}