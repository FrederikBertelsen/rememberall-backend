using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RememberAll.src.DTOs;
using RememberAll.src.DTOs.Create;
using RememberAll.src.Services.Interfaces;

namespace RememberAll.src.Controllers;

[Authorize]
[Route("api/lists")]
[ApiController]
public class TodoListController(ITodoListService todoListService) : ControllerBase
{
    [HttpPost("create")]
    public async Task<ActionResult<TodoListDto>> CreateTodoList(CreateTodoListDto newTodoListDto)
    {
        var createdTodoListDto = await todoListService.CreateTodoListAsync(newTodoListDto);
        return Ok(createdTodoListDto);
    }

    [HttpGet("by-id")]
    public async Task<ActionResult<TodoListDto>> GetTodoListById(Guid listId)
    {
        var todoListDto = await todoListService.GetTodoListByIdAsync(listId);
        return Ok(todoListDto);
    }

    [HttpGet("by-user")]
    public async Task<ActionResult<ICollection<TodoListDto>>> GetTodoListsByUserId()
    {
        var todoLists = await todoListService.GetTodoListsByUserIdAsync();
        return Ok(todoLists);
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteTodoList(Guid listId)
    {
        await todoListService.DeleteTodoList(listId);
        return NoContent();
    }
}