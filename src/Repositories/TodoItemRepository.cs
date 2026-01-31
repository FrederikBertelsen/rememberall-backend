using Microsoft.EntityFrameworkCore;
using RememberAll.src.Data;
using RememberAll.src.Entities;
using RememberAll.src.Repositories.Interfaces;

namespace RememberAll.src.Repositories;

public class TodoItemRepository(AppDbContext dbContext) : ITodoItemRepository
{
    public async Task<TodoItem> CreateTodoItemAsync(TodoItem todoItem) => (await dbContext.TodoItems.AddAsync(todoItem)).Entity;
    public async Task<TodoItem?> GetTodoItemByIdAsync(Guid todoItemId) => await dbContext.TodoItems.AsNoTracking().FirstOrDefaultAsync(t => t.Id == todoItemId);

    public TodoItem UpdateTodoItem(TodoItem todoItem) => dbContext.TodoItems.Update(todoItem).Entity;
    public TodoItem MarkTodoItemAsComplete(TodoItem todoItem)
    {
        todoItem.MarkAsComplete();

        return dbContext.TodoItems.Update(todoItem).Entity;
    }

    public TodoItem MarkTodoItemAsIncomplete(TodoItem todoItem)
    {
        todoItem.MarkAsIncomplete();

        return dbContext.TodoItems.Update(todoItem).Entity;
    }

    public void DeleteTodoItem(TodoItem todoItem) => dbContext.TodoItems.Remove(todoItem);

    public async Task SaveChangesAsync() => await dbContext.SaveChangesAsync();
}