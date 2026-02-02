using RememberAll.src.DTOs;
using RememberAll.src.DTOs.Create;

namespace RememberAll.src.Services.Interfaces;

public interface IInviteService
{
    public Task<InviteDto> CreateInviteAsync(CreateInviteDto createInviteDto);
    public Task<ICollection<InviteDto>> GetReceivedInvitesByUserAsync();
    public Task<ICollection<InviteDto>> GetSentInvitesByUserAsync();
    public Task AcceptInviteByIdAsync(Guid inviteId);
    public Task DeleteInviteByIdAsync(Guid inviteId);
}