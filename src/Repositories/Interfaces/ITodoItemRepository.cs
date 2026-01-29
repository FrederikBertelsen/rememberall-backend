using RememberAll.src.Entities;

namespace RememberAll.src.Repositories.Interfaces;

public interface ITodoItemRepository
{
    public Task<TodoItem> CreateTodoItemAsync(TodoItem todoItem);
    public TodoItem UpdateTodoItem(TodoItem todoItem);
    public void DeleteTodoItem(TodoItem todoItem);

    public Task SaveChangesAsync();

}