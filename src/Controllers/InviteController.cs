using Microsoft.AspNetCore.Mvc;
using RememberAll.src.DTOs;
using RememberAll.src.Services.Interfaces;

namespace RememberAll.src.Controllers;

[Route("api/invites")]
[ApiController]
public class InviteController(IInviteService inviteService) : ControllerBase
{
    [HttpPost("create")]
    public async Task<ActionResult<InviteDto>> CreateInvite(CreateInviteDto newInviteDto)
    {
        var createdInviteDto = await inviteService.CreateInviteAsync(newInviteDto);
        return Ok(createdInviteDto);
    }

    [HttpGet("sent")]
    public async Task<ActionResult<ICollection<InviteDto>>> GetSentInvitesByUserId(Guid userId)
    {
        var invites = await inviteService.GetSentInvitesByUserIdAsync(userId);
        return Ok(invites);
    }

    [HttpGet("received")]
    public async Task<ActionResult<ICollection<InviteDto>>> GetRecievedInvitesByUserId(Guid userId)
    {
        var invites = await inviteService.GetRecievedInvitesByUserIdAsync(userId);
        return Ok(invites);
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteInvite(Guid inviteId)
    {
        await inviteService.DeleteInviteByIdAsync(inviteId);
        return NoContent();
    }
}