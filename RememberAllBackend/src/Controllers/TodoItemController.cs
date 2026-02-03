using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RememberAll.src.DTOs;
using RememberAll.src.DTOs.Create;
using RememberAll.src.DTOs.Update;
using RememberAll.src.Services.Interfaces;

namespace RememberAll.src.Controllers;

[Authorize]
[Route("api/todoitems")]
[ApiController]
public class TodoItemController(ITodoItemService todoItemService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TodoItemDto>> CreateTodoItem(CreateTodoItemDto newTodoItemDto)
    {
        var createdTodoItemDto = await todoItemService.CreateTodoItemAsync(newTodoItemDto);
        return Ok(createdTodoItemDto);
    }

    [HttpGet("bylist/{listId}")]
    public async Task<ActionResult<ICollection<TodoItemDto>>> GetTodoItemsByListId(Guid listId)
    {
        var todoItems = await todoItemService.GetTodoItemsByListIdAsync(listId);
        return Ok(todoItems);
    }

    [HttpPatch]
    public async Task<ActionResult<TodoItemDto>> UpdateTodoItem(UpdateTodoItemDto updateTodoItemDto)
    {
        var updatedTodoItemDto = await todoItemService.UpdateTodoItemAsync(updateTodoItemDto);
        return Ok(updatedTodoItemDto);
    }

    [HttpPatch("{itemId}/complete")]
    public async Task<ActionResult<TodoItemDto>> MarkTodoItemAsComplete(Guid itemId)
    {
        var updatedTodoItemDto = await todoItemService.MarkTodoItemAsCompleteAsync(itemId);
        return Ok(updatedTodoItemDto);
    }

    [HttpPatch("{itemId}/incomplete")]
    public async Task<ActionResult<TodoItemDto>> MarkTodoItemAsIncomplete(Guid itemId)
    {
        var updatedTodoItemDto = await todoItemService.MarkTodoItemAsIncompleteAsync(itemId);
        return Ok(updatedTodoItemDto);
    }

    [HttpDelete("{itemId}")]
    public async Task<IActionResult> DeleteTodoItem(Guid itemId)
    {
        await todoItemService.DeleteTodoItemAsync(itemId);
        return NoContent();
    }
}