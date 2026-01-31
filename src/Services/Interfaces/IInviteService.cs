using RememberAll.src.DTOs;

namespace RememberAll.src.Services.Interfaces;

public interface IInviteService
{
    public Task<InviteDto> CreateInviteAsync(CreateInviteDto createInviteDto);
    public Task<ICollection<InviteDto>> GetRecievedInvitesByUserIdAsync();
    public Task<ICollection<InviteDto>> GetSentInvitesByUserIdAsync();
    public Task AcceptInviteByIdAsync(Guid inviteId);
    public Task DeleteInviteByIdAsync(Guid inviteId);
}