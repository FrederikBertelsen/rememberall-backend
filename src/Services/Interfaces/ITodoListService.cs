using RememberAll.src.DTOs;

namespace RememberAll.src.Services.Interfaces;

public interface ITodoListService
{
    public Task<TodoListDto> CreateTodoListAsync(CreateTodoListDto createTodoListDto);
    public Task<TodoListDto?> GetTodoListByIdAsync(Guid listId);
    public Task<ICollection<TodoListDto>> GetTodoListsByUserIdAsync(Guid userId);
    public Task DeleteTodoList(Guid listId);
}