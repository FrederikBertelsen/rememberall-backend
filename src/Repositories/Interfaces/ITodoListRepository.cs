using RememberAll.src.Entities;

namespace RememberAll.src.Repositories.Interfaces;

public interface ITodoListRepository
{
    public Task<TodoList> CreateTodoListAsync(TodoList todoList);
    public Task<TodoList?> GetTodoListByIdAsync(Guid listId);
    public Task<ICollection<TodoList>> GetTodoListsByUserIdAsync(Guid userId);
    public void DeleteTodoList(TodoList todoList);

    public Task SaveChangesAsync();

}