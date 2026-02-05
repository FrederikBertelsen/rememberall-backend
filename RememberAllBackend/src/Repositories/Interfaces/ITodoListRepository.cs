using RememberAll.src.Entities;

namespace RememberAll.src.Repositories.Interfaces;

public interface ITodoListRepository
{
    public Task<TodoList> CreateTodoListAsync(TodoList todoList);
    public Task<TodoList?> GetTodoListByIdAsync(Guid listId);
    public Task<ICollection<TodoList>> GetTodoListsByUserIdAsync(Guid userId);
    public Task<DateTime> GetTodoListUpdatedAtAsync(Guid listId);
    public Task<bool> TodoListExistsByIdAsync(Guid listId);
    public TodoList UpdateTodoList(TodoList todoList);
    public void DeleteTodoList(TodoList todoList);

    public Task SaveChangesAsync();

}