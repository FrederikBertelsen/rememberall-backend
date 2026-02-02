using RememberAll.src.DTOs;
using RememberAll.src.DTOs.Create;
using RememberAll.src.DTOs.Update;
using RememberAll.src.Entities;
using RememberAll.src.Exceptions;
using RememberAll.src.Extensions;
using RememberAll.src.Repositories.Interfaces;
using RememberAll.src.Services.Interfaces;

namespace RememberAll.src.Services;

public class TodoListService(
    IUserRepository userRepository,
    ITodoListRepository todoListRepository,
    IListAccessRepository listAccessRepository,
    ICurrentUserService currentUserService) : ITodoListService
{
    public async Task<TodoListDto> CreateTodoListAsync(CreateTodoListDto createTodoListDto)
    {
        createTodoListDto.ValidateOrThrow();

        var userId = currentUserService.GetUserId();
        User owner = await userRepository.GetUserByIdAsync(userId)
            ?? throw new NotFoundException("User", "Id", userId);

        TodoList newTodoList = createTodoListDto.ToEntity(owner);
        TodoListDto todoListDto = (await todoListRepository.CreateTodoListAsync(newTodoList)).ToDto();

        await todoListRepository.SaveChangesAsync();

        return todoListDto;
    }

    public async Task<TodoListDto?> GetTodoListByIdAsync(Guid listId)
    {
        if (listId == Guid.Empty)
            throw new MissingValueException("List Id");

        TodoList? todoList = await todoListRepository.GetTodoListByIdAsync(listId)
            ?? throw new NotFoundException("List", "Id", listId);

        if (!await listAccessRepository.UserHasAccessToListAsync(currentUserService.GetUserId(), listId))
            throw new AuthException("User does not have access to this list.");

        return todoList.ToDto();
    }

    public async Task<ICollection<TodoListDto>> GetTodoListsByUserIdAsync()
    {
        var userId = currentUserService.GetUserId();
        if (!await userRepository.UserExistsByIdAsync(userId))
            throw new NotFoundException("User", "Id", userId);

        ICollection<TodoList> todoLists = await todoListRepository.GetTodoListsByUserIdAsync(userId);

        return todoLists.ToDtos();
    }

    public async Task<TodoListDto> UpdateTodoListAsync(UpdateTodoListDto updateTodoListDto)
    {
        TodoList todoList = await todoListRepository.GetTodoListByIdAsync(updateTodoListDto.Id)
            ?? throw new NotFoundException("List", "Id", updateTodoListDto.Id);
        
        if (!await listAccessRepository.UserHasAccessToListAsync(currentUserService.GetUserId(), todoList.Id))
            throw new UnauthorizedAccessException("User does not have access to the specified Todo List");
        
        todoList.ApplyNonNullValuesFromDto(updateTodoListDto);
        todoListRepository.UpdateTodoList(todoList);

        await todoListRepository.SaveChangesAsync();

        return todoList.ToDto();
    }

    public async Task DeleteTodoList(Guid listId)
    {
        if (listId == Guid.Empty)
            throw new MissingValueException("List Id");

        TodoList? todoList = await todoListRepository.GetTodoListByIdAsync(listId)
            ?? throw new NotFoundException("List", "Id", listId);

        if (todoList.OwnerId != currentUserService.GetUserId())
            throw new AuthException("User is not the owner of this list.");

        todoListRepository.DeleteTodoList(todoList);

        await todoListRepository.SaveChangesAsync();
    }
}