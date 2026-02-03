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
    [HttpGet]
    public async Task<ActionResult<ICollection<ListAccessDto>>> GetListAccess([FromQuery] Guid? listId = null)
    {
        if (listId.HasValue)
        {
            var listAccesses = await listAccessService.GetListAccesssByListIdAsync(listId.Value);
            return Ok(listAccesses);
        }
        else
        {
            var listAccesses = await listAccessService.GetListAccesssByUserAsync();
            return Ok(listAccesses);
        }
    }

    [HttpDelete("{listAccessId}")]
    public async Task<IActionResult> DeleteListAccess(Guid listAccessId)
    {
        await listAccessService.DeleteListAccessAsync(listAccessId);
        return NoContent();
    }
}
