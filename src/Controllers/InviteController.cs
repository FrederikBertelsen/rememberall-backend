using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RememberAll.src.DTOs;
using RememberAll.src.Services.Interfaces;

namespace RememberAll.src.Controllers;

[Authorize]
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
    public async Task<ActionResult<ICollection<InviteDto>>> GetSentInvitesByUserId()
    {
        var invites = await inviteService.GetSentInvitesByUserIdAsync();
        return Ok(invites);
    }

    [HttpGet("received")]
    public async Task<ActionResult<ICollection<InviteDto>>> GetRecievedInvitesByUserId()
    {
        var invites = await inviteService.GetRecievedInvitesByUserIdAsync();
        return Ok(invites);
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteInvite(Guid inviteId)
    {
        await inviteService.DeleteInviteByIdAsync(inviteId);
        return NoContent();
    }
}