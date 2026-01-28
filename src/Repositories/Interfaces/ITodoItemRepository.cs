using rememberall.src.Entities;

namespace rememberall.src.Repositories.Interfaces;

public interface ITodoItemRepository
{
    public Task<TodoItem> CreateTodoItemAsync(TodoItem todoItem);
    public void DeleteTodoItem(TodoItem todoItem);

    public Task SaveChangesAsync();

}