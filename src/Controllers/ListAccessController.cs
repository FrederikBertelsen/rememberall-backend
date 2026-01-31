using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RememberAll.src.DTOs;
using RememberAll.src.Services.Interfaces;

namespace RememberAll.src.Controllers;

[Authorize]
[Route("api/listaccess")]
[ApiController]
public class ListAccessController(IListAccessService listAccessService) : ControllerBase
{
    [HttpGet("by-list")]
    public async Task<ActionResult<ICollection<ListAccessDto>>> GetListAccessByListId(Guid listId)
    {
        var listAccesses = await listAccessService.GetListAccesssByListIdAsync(listId);
        return Ok(listAccesses);
    }

    [HttpGet("by-user")]
    public async Task<ActionResult<ICollection<ListAccessDto>>> GetListAccessByUser()
    {
        var listAccesses = await listAccessService.GetListAccesssByUserAsync();
        return Ok(listAccesses);
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteListAccess(Guid listAccessId)
    {
        await listAccessService.DeleteListAccessAsync(listAccessId);
        return NoContent();
    }
}
