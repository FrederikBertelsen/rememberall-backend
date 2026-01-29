using Microsoft.AspNetCore.Mvc;
using RememberAll.src.DTOs;
using RememberAll.src.Services.Interfaces;

namespace RememberAll.src.Controllers;

[Route("api/listaccess")]
[ApiController]
public class ListAccessController(IListAccessService listAccessService) : ControllerBase
{
    [HttpPost("create")]
    public async Task<ActionResult<ListAccessDto>> CreateListAccess(CreateListAccessDto newListAccessDto)
    {
        var createdListAccessDto = await listAccessService.CreateListAccessAsync(newListAccessDto);
        return Ok(createdListAccessDto);
    }

    [HttpGet("bylist")]
    public async Task<ActionResult<ICollection<ListAccessDto>>> GetListAccessByListId(Guid listId)
    {
        var listAccesses = await listAccessService.GetListAccesssByListIdAsync(listId);
        return Ok(listAccesses);
    }

    [HttpGet("byuser")]
    public async Task<ActionResult<ICollection<ListAccessDto>>> GetListAccessByUserId(Guid userId)
    {
        var listAccesses = await listAccessService.GetListAccesssByUserIdAsync(userId);
        return Ok(listAccesses);
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteListAccess(Guid listAccessId)
    {
        await listAccessService.DeleteListAccessAsync(listAccessId);
        return NoContent();
    }
}
