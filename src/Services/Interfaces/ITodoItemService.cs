using RememberAll.src.DTOs;

namespace RememberAll.src.Services.Interfaces;

public interface ITodoService
{
    public Task<TodoItemDto> CreateTodoItemAsync(CreateTodoItemDto createTodoItemDto);
    public Task<TodoItemDto> UpdateTodoItem(TodoItemDto todoItemDto);

    public Task<TodoItemDto> MarkTodoItemAsCompleteAsync(Guid todoItemId);
    public Task<TodoItemDto> MarkTodoItemAsIncompleteAsync(Guid todoItemId);
    public Task DeleteTodoItem(Guid todoItemId);
}