using RememberAll.src.DTOs;

namespace RememberAll.src.Services.Interfaces;

public interface ITodoService
{
    public Task<TodoItemDto> CreateTodoItemAsync(CreateTodoItemDto createTodoItemDto);
    public Task<TodoItemDto> UpdateTodoItem(TodoItemDto todoItemDto);
    public Task DeleteTodoItem(Guid todoItemId);
}