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
    public async Task<ICollection<TodoList>> GetTodoListsByUserIdAsync(Guid userId)
    {
        var ownedLists = await dbContext.TodoLists
            .AsNoTracking()
            .Where(list => list.OwnerId == userId)
            .ToListAsync();

        var sharedLists = await dbContext.ListAccess
            .AsNoTracking()
            .Where(access => access.UserId == userId)
            .Select(access => access.List)
            .Distinct()
            .ToListAsync();

        var allLists = ownedLists
            .Concat(sharedLists)
            .ToList();

        return allLists;
    }

    public void DeleteTodoList(TodoList todoList) => dbContext.TodoLists.Remove(todoList);
    public async Task SaveChangesAsync() => await dbContext.SaveChangesAsync();
}