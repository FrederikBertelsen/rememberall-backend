using Microsoft.AspNetCore.Mvc;
using RememberAll.src.DTOs;
using RememberAll.src.Services.Interfaces;

namespace RememberAll.src.Controllers;

[Route("api/todolists")]
[ApiController]
public class TodoListController(ITodoListService todoListService) : ControllerBase
{
    [HttpPost("create")]
    public async Task<ActionResult<TodoListDto>> CreateTodoList(CreateTodoListDto newTodoListDto)
    {
        var createdTodoListDto = await todoListService.CreateTodoListAsync(newTodoListDto);
        return Ok(createdTodoListDto);
    }

    [HttpGet("bylist")]
    public async Task<ActionResult<TodoListDto>> GetTodoListById(Guid listId)
    {
        var todoListDto = await todoListService.GetTodoListByIdAsync(listId);
        return Ok(todoListDto);
    }

    [HttpGet("byuser")]
    public async Task<ActionResult<ICollection<TodoListDto>>> GetTodoListsByUserId(Guid userId)
    {
        var todoLists =  await todoListService.GetTodoListsByUserIdAsync(userId);
        return Ok(todoLists);
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteTodoList(Guid listId)
    {
        await todoListService.DeleteTodoList(listId);
        return NoContent();
    }
}