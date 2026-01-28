using rememberall.src.Data;
using rememberall.src.Entities;
using rememberall.src.Repositories.Interfaces;

namespace rememberall.src.Repositories;

public class TodoItemRepository(AppDbContext dbContext) : ITodoItemRepository
{
    public async Task<TodoItem> CreateTodoItemAsync(TodoItem todoItem) => (await dbContext.TodoItems.AddAsync(todoItem)).Entity;
    public void DeleteTodoItem(TodoItem todoItem) => dbContext.TodoItems.Remove(todoItem);

    public async Task SaveChangesAsync() => await dbContext.SaveChangesAsync();
}