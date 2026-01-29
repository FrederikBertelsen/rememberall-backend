using RememberAll.src.DTOs;
using RememberAll.src.Entities;
using RememberAll.src.Exceptions;
using RememberAll.src.Extensions;
using RememberAll.src.Repositories.Interfaces;
using RememberAll.src.Services.Interfaces;

namespace RememberAll.src.Services;

public class TodoListService(IUserRepository userRepository, ITodoListRepository todoListRepository) : ITodoListService
{
    public async Task<TodoListDto> CreateTodoListAsync(CreateTodoListDto createTodoListDto)
    {
        createTodoListDto.ValidateOrThrow();

        User? owner = await userRepository.GetUserByIdAsync(createTodoListDto.OwnerId)
            ?? throw new NotFoundException("User", "Id", createTodoListDto.OwnerId);

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

        return todoList.ToDto();
    }

    public async Task<ICollection<TodoListDto>> GetTodoListsByUserIdAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new MissingValueException("User Id");

        if (!await userRepository.UserExistsByIdAsync(userId))
            throw new NotFoundException("User", "Id", userId);

        ICollection<TodoList> todoLists = await todoListRepository.GetTodoListsByUserIdAsync(userId);

        return todoLists.ToDtos();
    }

    public async Task DeleteTodoList(TodoListDto todoListDto)
    {
        todoListDto.ValidateOrThrow();

        TodoList? todoList = await todoListRepository.GetTodoListByIdAsync(todoListDto.Id)
            ?? throw new NotFoundException("List", "Id", todoListDto.Id);

        todoListRepository.DeleteTodoList(todoList);

        await todoListRepository.SaveChangesAsync();
    }
}