using RememberAll.src.DTOs;
using RememberAll.src.DTOs.Create;
using RememberAll.src.DTOs.Update;

namespace RememberAll.src.Services.Interfaces;

public interface ITodoItemService
{
    public Task<TodoItemDto> CreateTodoItemAsync(CreateTodoItemDto createTodoItemDto);
    public Task<TodoItemDto> UpdateTodoItem(UpdateTodoItemDto updateTodoItemDto);

    public Task<TodoItemDto> MarkTodoItemAsCompleteAsync(Guid todoItemId);
    public Task<TodoItemDto> MarkTodoItemAsIncompleteAsync(Guid todoItemId);
    public Task DeleteTodoItem(Guid todoItemId);
}