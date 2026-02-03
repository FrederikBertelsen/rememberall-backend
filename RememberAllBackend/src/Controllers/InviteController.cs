using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RememberAll.src.DTOs;
using RememberAll.src.DTOs.Create;
using RememberAll.src.Services.Interfaces;

namespace RememberAll.src.Controllers;

[Authorize]
[Route("api/invites")]
[ApiController]
public class InviteController(IInviteService inviteService) : ControllerBase
{

    [HttpPost]
    public async Task<ActionResult<InviteDto>> CreateInvite(CreateInviteDto createInviteDto)
    {
        var createdInviteDto = await inviteService.CreateInviteAsync(createInviteDto);
        return Ok(createdInviteDto);
    }

    [HttpGet("sent")]
    public async Task<ActionResult<ICollection<InviteDto>>> GetSentInvitesByUser()
    {
        var invites = await inviteService.GetSentInvitesByUserAsync();
        return Ok(invites);
    }

    [HttpGet("received")]
    public async Task<ActionResult<ICollection<InviteDto>>> GetReceivedInvitesByUser()
    {
        var invites = await inviteService.GetReceivedInvitesByUserAsync();
        return Ok(invites);
    }

    [HttpPatch("{inviteId}/accept")]
    public async Task<IActionResult> AcceptInvite(Guid inviteId)
    {
        await inviteService.AcceptInviteByIdAsync(inviteId);
        return NoContent();
    }

    [HttpDelete("{inviteId}")]
    public async Task<IActionResult> DeleteInvite(Guid inviteId)
    {
        await inviteService.DeleteInviteByIdAsync(inviteId);
        return NoContent();
    }
}