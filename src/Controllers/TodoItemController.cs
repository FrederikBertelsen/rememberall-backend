using Microsoft.AspNetCore.Mvc;
using RememberAll.src.DTOs;
using RememberAll.src.Services.Interfaces;

namespace RememberAll.src.Controllers;

[Route("api/todoitems")]
[ApiController]
public class TodoItemController(ITodoItemService todoItemService) : ControllerBase
{
    [HttpPost("create")]
    public async Task<ActionResult<TodoItemDto>> CreateTodoItem(CreateTodoItemDto newTodoItemDto)
    {
        var createdTodoItemDto = await todoItemService.CreateTodoItemAsync(newTodoItemDto);
        return Ok(createdTodoItemDto);
    }

    [HttpPatch("update")]
    public async Task<ActionResult<TodoItemDto>> UpdateTodoItem(TodoItemDto todoItemDto)
    {
        var updatedTodoItemDto = await todoItemService.UpdateTodoItem(todoItemDto);
        return Ok(updatedTodoItemDto);
    }

    [HttpPatch("markcomplete")]
    public async Task<ActionResult<TodoItemDto>> MarkTodoItemAsComplete(Guid itemId)
    {
        var updatedTodoItemDto = await todoItemService.MarkTodoItemAsCompleteAsync(itemId);
        return Ok(updatedTodoItemDto);
    }

    [HttpPatch("markincomplete")]
    public async Task<ActionResult<TodoItemDto>> MarkTodoItemAsIncomplete(Guid itemId)
    {
        var updatedTodoItemDto = await todoItemService.MarkTodoItemAsIncompleteAsync(itemId);
        return Ok(updatedTodoItemDto);
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteTodoItem(Guid itemId)
    {
        await todoItemService.DeleteTodoItem(itemId);
        return NoContent();
    }
}