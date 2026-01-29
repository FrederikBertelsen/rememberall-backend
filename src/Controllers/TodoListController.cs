using Microsoft.AspNetCore.Mvc;
using RememberAll.src.DTOs;
using RememberAll.src.Services.Interfaces;

namespace RememberAll.src.Controllers;

public class TodoListController(ITodoListService todoListService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TodoListDto>> CreateTodoList(CreateTodoListDto newTodoListDto)
    {
        var createdTodoListDto = await todoListService.CreateTodoListAsync(newTodoListDto);
        return Ok(createdTodoListDto);
    }

    [HttpGet]
    public async Task<ActionResult<TodoListDto>> GetTodoListById(Guid listId)
    {
        var todoListDto = await todoListService.GetTodoListByIdAsync(listId);
        return Ok(todoListDto);
    }

    [HttpGet]
    public async Task<ActionResult<ICollection<TodoListDto>>> GetTodoListsByUserId(Guid userId)
    {
        var todoLists =  await todoListService.GetTodoListsByUserIdAsync(userId);
        return Ok(todoLists);
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteTodoList(Guid listId)
    {
        await todoListService.DeleteTodoList(listId);
        return NoContent();
    }
}