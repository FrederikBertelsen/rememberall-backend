using Microsoft.EntityFrameworkCore;
using RememberAll.src.Data;
using RememberAll.src.Entities;
using RememberAll.src.Repositories.Interfaces;

namespace RememberAll.src.Repositories;

public class TodoListRepository(AppDbContext dbContext) : ITodoListRepository
{
    public async Task<TodoList> CreateTodoListAsync(TodoList todoList) =>
        (await dbContext.TodoLists.AddAsync(todoList)).Entity;
    public async Task<TodoList?> GetTodoListByIdAsync(Guid listId) =>
        await dbContext.TodoLists
                .AsNoTracking()
                .Include(todoList => todoList.Items)
                .FirstOrDefaultAsync(todoList => todoList.Id == listId);
    public async Task<ICollection<TodoList>> GetTodoListsByUserIdAsync(Guid userId) =>
        await dbContext.TodoLists
                .AsNoTracking()
                .Where(todoList => todoList.OwnerId == userId)
                .ToListAsync();

    public void DeleteTodoList(TodoList todoList) => dbContext.TodoLists.Remove(todoList);
    public async Task SaveChangesAsync() => await dbContext.SaveChangesAsync();
}