using rememberall.src.Entities;

namespace rememberall.src.Repositories.Interfaces;

public interface ITodoListRepository
{
    public Task<TodoList> CreateTodoListAsync(TodoList todoList);
    public Task<TodoList?> GetTodoListByIdAsync(Guid listId);
    public TodoList UpdateTodoList(TodoList todoList);
    public Task<ICollection<TodoList>> GetTodoListsByUserIdAsync(Guid userId);
    public void DeleteTodoList(TodoList todoList);

    public Task SaveChangesAsync();

}