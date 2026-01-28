using Microsoft.EntityFrameworkCore;
using rememberall.src.Data;
using rememberall.src.Entities;
using rememberall.src.Repositories.Interfaces;

namespace rememberall.src.Repositories;

public class TodoListRepository(AppDbContext dbContext) : ITodoListRepository
{
    public async Task<TodoList> CreateTodoListAsync(TodoList todoList) => 
        (await dbContext.TodoLists.AddAsync(todoList)).Entity;
    public async Task<TodoList?> GetTodoListByIdAsync(Guid listId) => 
        await dbContext.TodoLists.Include(todoList => todoList.Items).FirstOrDefaultAsync(todoList => todoList.Id == listId);
    public async Task<ICollection<TodoList>> GetTodoListsByUserIdAsync(Guid userId) => 
        await dbContext.TodoLists.Where(todoList => todoList.OwnerId == userId).ToListAsync();
    public void AddItemToTodoList(TodoList todoList, TodoItem todoItem) => todoList.Items.Add(todoItem);
    public void RemoveItemFromTodoList(TodoList todoList, TodoItem todoItem) => todoList.Items.Remove(todoItem);

    public void DeleteTodoList(TodoList todoList) => dbContext.TodoLists.Remove(todoList);
    public async Task SaveChangesAsync() => await dbContext.SaveChangesAsync();
}