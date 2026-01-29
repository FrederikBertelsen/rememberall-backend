using RememberAll.src.DTOs;
using RememberAll.src.Entities;
using RememberAll.src.Exceptions;
using RememberAll.src.Extensions;
using RememberAll.src.Repositories.Interfaces;
using RememberAll.src.Services.Interfaces;

namespace RememberAll.src.Services;

public class TodoItemService(ITodoListRepository todoListRepository, ITodoItemRepository todoItemRepository) : ITodoService
{
    public async Task<TodoItemDto> CreateTodoItemAsync(CreateTodoItemDto createTodoItemDto)
    {
        createTodoItemDto.ValidateOrThrow();

        TodoList todoList = await todoListRepository.GetTodoListByIdAsync(createTodoItemDto.TodoListId)
            ?? throw new NotFoundException("List", "Id", createTodoItemDto.TodoListId);

        TodoItem newTodoItem = createTodoItemDto.ToEntity(todoList);
        TodoItemDto todoItemDto = (await todoItemRepository.CreateTodoItemAsync(newTodoItem)).ToDto();

        await todoItemRepository.SaveChangesAsync();

        return todoItemDto;
    }

    public async Task<TodoItemDto> UpdateTodoItem(TodoItemDto todoItemDto)
    {
        todoItemDto.ValidateOrThrow();

        TodoItem todoItem = await todoItemRepository.GetTodoItemByIdAsync(todoItemDto.Id)
            ?? throw new NotFoundException("Todo Item", "Id", todoItemDto.Id);

        todoItem.ApplyNonNullValuesFromDto(todoItemDto);
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

        todoItemRepository.DeleteTodoItem(todoItem);

        await todoItemRepository.SaveChangesAsync();
    }
}