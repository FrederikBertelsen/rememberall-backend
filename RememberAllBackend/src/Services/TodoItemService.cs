using RememberAll.src.DTOs;
using RememberAll.src.DTOs.Create;
using RememberAll.src.DTOs.Update;
using RememberAll.src.Entities;
using RememberAll.src.Exceptions;
using RememberAll.src.Extensions;
using RememberAll.src.Repositories.Interfaces;
using RememberAll.src.Services.Interfaces;

namespace RememberAll.src.Services;

public class TodoItemService(
    ITodoListRepository todoListRepository,
    ITodoItemRepository todoItemRepository,
    IListAccessRepository listAccessRepository,
    ICurrentUserService currentUserService) : ITodoItemService
{
    public async Task<TodoItemDto> CreateTodoItemAsync(CreateTodoItemDto createTodoItemDto)
    {
        createTodoItemDto.ValidateOrThrow();

        TodoList todoList = await todoListRepository.GetTodoListByIdAsync(createTodoItemDto.TodoListId)
            ?? throw new NotFoundException("List", "Id", createTodoItemDto.TodoListId);

        if (!await listAccessRepository.UserHasAccessToListAsync(currentUserService.GetUserId(), todoList.Id))
            throw new UnauthorizedAccessException("User does not have access to the specified Todo List");

        TodoItem newTodoItem = createTodoItemDto.ToEntity(todoList);
        TodoItemDto todoItemDto = (await todoItemRepository.CreateTodoItemAsync(newTodoItem)).ToDto();

        await todoItemRepository.SaveChangesAsync();

        return todoItemDto;
    }

    public async Task<TodoItemDto> UpdateTodoItem(UpdateTodoItemDto updateTodoItemDto)
    {
        updateTodoItemDto.ValidateOrThrow();

        TodoItem todoItem = await todoItemRepository.GetTodoItemByIdAsync(updateTodoItemDto.Id)
            ?? throw new NotFoundException("Todo Item", "Id", updateTodoItemDto.Id);

        if (!await listAccessRepository.UserHasAccessToListAsync(currentUserService.GetUserId(), todoItem.TodoListId))
            throw new UnauthorizedAccessException("User does not have access to the specified Todo List");

        todoItem.ApplyNonNullValuesFromDto(updateTodoItemDto);
        todoItemRepository.UpdateTodoItem(todoItem);

        await todoItemRepository.SaveChangesAsync();

        return todoItem.ToDto();
    }

    public async Task<TodoItemDto> MarkTodoItemAsCompleteAsync(Guid todoItemId)
    {
        if (todoItemId == Guid.Empty)
            throw new MissingValueException("TodoItem Id");

        TodoItem todoItem = await todoItemRepository.GetTodoItemByIdAsync(todoItemId)
            ?? throw new NotFoundException("Todo Item", "Id", todoItemId);

        if (!await listAccessRepository.UserHasAccessToListAsync(currentUserService.GetUserId(), todoItem.TodoListId))
            throw new UnauthorizedAccessException("User does not have access to the specified Todo List");

        if (todoItem.IsCompleted)
            throw new BusinessLogicException("TodoItem is already completed");

        todoItemRepository.MarkTodoItemAsComplete(todoItem);

        await todoItemRepository.SaveChangesAsync();

        return todoItem.ToDto();
    }

    public async Task<TodoItemDto> MarkTodoItemAsIncompleteAsync(Guid todoItemId)
    {
        if (todoItemId == Guid.Empty)
            throw new MissingValueException("TodoItem Id");

        TodoItem todoItem = await todoItemRepository.GetTodoItemByIdAsync(todoItemId)
            ?? throw new NotFoundException("Todo Item", "Id", todoItemId);

        if (!await listAccessRepository.UserHasAccessToListAsync(currentUserService.GetUserId(), todoItem.TodoListId))
            throw new UnauthorizedAccessException("User does not have access to the specified Todo List");

        if (!todoItem.IsCompleted)
            throw new BusinessLogicException("TodoItem is already incomplete");

        todoItemRepository.MarkTodoItemAsIncomplete(todoItem);

        await todoItemRepository.SaveChangesAsync();

        return todoItem.ToDto();
    }

    public async Task DeleteTodoItem(Guid todoItemId)
    {
        TodoItem todoItem = await todoItemRepository.GetTodoItemByIdAsync(todoItemId)
            ?? throw new NotFoundException("Todo Item", "Id", todoItemId);

        if (!await listAccessRepository.UserHasAccessToListAsync(currentUserService.GetUserId(), todoItem.TodoListId))
            throw new UnauthorizedAccessException("User does not have access to the specified Todo List");

        todoItemRepository.DeleteTodoItem(todoItem);

        await todoItemRepository.SaveChangesAsync();
    }
}