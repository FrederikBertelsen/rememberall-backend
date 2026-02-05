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
            .Include(listAccess => listAccess.List)
            .AsNoTracking()
            .Where(access => access.UserId == userId)
            .Select(access => access.List!)
            .Distinct()
            .ToListAsync();

        var allLists = ownedLists
            .Concat(sharedLists)
            .ToList();

        return allLists;
    }

    public async Task<DateTime> GetTodoListUpdatedAtAsync(Guid listId) =>
        await dbContext.TodoLists
            .Where(list => list.Id == listId)
            .Select(list => list.UpdatedAt)
            .FirstOrDefaultAsync();

    public Task<bool> TodoListExistsByIdAsync(Guid listId) =>
        dbContext.TodoLists.AsNoTracking().AnyAsync(list => list.Id == listId);

    public TodoList UpdateTodoList(TodoList todoList) => dbContext.TodoLists.Update(todoList).Entity;

    public void DeleteTodoList(TodoList todoList) => dbContext.TodoLists.Remove(todoList);
    public async Task SaveChangesAsync() => await dbContext.SaveChangesAsync();
}