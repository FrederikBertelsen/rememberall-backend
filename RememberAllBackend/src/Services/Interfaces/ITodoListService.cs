using RememberAll.src.DTOs;
using RememberAll.src.DTOs.Create;
using RememberAll.src.DTOs.Update;

namespace RememberAll.src.Services.Interfaces;

public interface ITodoListService
{
    public Task<TodoListDto> CreateTodoListAsync(CreateTodoListDto createTodoListDto);
    public Task<TodoListDto?> GetTodoListByIdAsync(Guid listId);
    public Task<ICollection<TodoListDto>> GetTodoListsByUserIdAsync();
    public Task<TodoListDto?> RefreshTodoListAsync(Guid listId, DateTime currentUpdatedAt);
    public Task<TodoListDto> UpdateTodoListAsync(UpdateTodoListDto updateTodoListDto);
    public Task DeleteTodoListAsync(Guid listId);
}