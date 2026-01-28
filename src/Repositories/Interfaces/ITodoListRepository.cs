using rememberall.src.Entities;

namespace rememberall.src.Repositories.Interfaces;

public interface ITodoListRepository
{
    public Task<TodoList> CreateTodoListAsync(TodoList todoList);
    public Task<TodoList?> GetTodoListByIdAsync(Guid listId);
    public Task<ICollection<TodoList>> GetTodoListsByUserIdAsync(Guid userId);
    public void AddItemToTodoList(TodoList todoList, TodoItem todoItem);
    public void RemoveItemFromTodoList(TodoList todoList, TodoItem todoItem);

    public void DeleteTodoList(TodoList todoList);

    public Task SaveChangesAsync();

}