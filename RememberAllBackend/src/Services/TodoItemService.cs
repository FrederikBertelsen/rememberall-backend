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
            throw new ForbiddenException("User does not have access to the specified Todo List");

        TodoItem newTodoItem = createTodoItemDto.ToEntity(todoList);
        TodoItemDto todoItemDto = (await todoItemRepository.CreateTodoItemAsync(newTodoItem)).ToDto();

        await todoItemRepository.SaveChangesAsync();

        return todoItemDto;
    }

    public async Task<ICollection<TodoItemDto>> GetTodoItemsByListIdAsync(Guid todoListId)
    {
        if (todoListId == Guid.Empty)
            throw new MissingValueException("TodoList Id");

        if (!await listAccessRepository.UserHasAccessToListAsync(currentUserService.GetUserId(), todoListId))
            throw new ForbiddenException("User does not have access to the specified Todo List");

        ICollection<TodoItem> todoItems = await todoItemRepository.GetTodoItemsByListIdAsync(todoListId);

        return todoItems.ToDtos();
    }

    public async Task<TodoItemDto> UpdateTodoItemAsync(UpdateTodoItemDto updateTodoItemDto)
    {
        updateTodoItemDto.ValidateOrThrow();

        TodoItem todoItem = await todoItemRepository.GetTodoItemByIdAsync(updateTodoItemDto.Id)
            ?? throw new NotFoundException("Todo Item", "Id", updateTodoItemDto.Id);

        if (!await listAccessRepository.UserHasAccessToListAsync(currentUserService.GetUserId(), todoItem.TodoListId))
            throw new ForbiddenException("User does not have access to the specified Todo List");

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
            throw new ForbiddenException("User does not have access to the specified Todo List");

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
            throw new ForbiddenException("User does not have access to the specified Todo List");

        if (!todoItem.IsCompleted)
            throw new BusinessLogicException("TodoItem is already incomplete");

        todoItemRepository.MarkTodoItemAsIncomplete(todoItem);

        await todoItemRepository.SaveChangesAsync();

        return todoItem.ToDto();
    }

    public async Task DeleteTodoItemAsync(Guid todoItemId)
    {
        TodoItem todoItem = await todoItemRepository.GetTodoItemByIdAsync(todoItemId)
            ?? throw new NotFoundException("Todo Item", "Id", todoItemId);

        if (!await listAccessRepository.UserHasAccessToListAsync(currentUserService.GetUserId(), todoItem.TodoListId))
            throw new ForbiddenException("User does not have access to the specified Todo List");

        todoItemRepository.DeleteTodoItem(todoItem);

        await todoItemRepository.SaveChangesAsync();
    }

    public async Task<BatchUpdateTodoItemsResultDto> BatchUpdateTodoItemsAsync(BatchUpdateTodoItemsDto batchUpdateDto)
    {
        // Validate list ID
        if (batchUpdateDto.TodoListId == Guid.Empty)
            throw new MissingValueException("TodoList Id");

        // Verify list exists
        TodoList todoList = await todoListRepository.GetTodoListByIdAsync(batchUpdateDto.TodoListId)
            ?? throw new NotFoundException("List", "Id", batchUpdateDto.TodoListId);

        // Check access once for the entire batch
        if (!await listAccessRepository.UserHasAccessToListAsync(currentUserService.GetUserId(), todoList.Id))
            throw new ForbiddenException("User does not have access to the specified Todo List");

        // Prepare result collections
        var createdItems = new List<TodoItemDto>();
        var updatedItems = new List<TodoItemDto>();
        var completedItems = new List<TodoItemDto>();
        var incompletedItems = new List<TodoItemDto>();
        var deletedIds = new List<Guid>();

        // Process creates
        if (batchUpdateDto.Creates != null)
        {
            foreach (var createDto in batchUpdateDto.Creates)
            {
                if (string.IsNullOrWhiteSpace(createDto.Text))
                    throw new MissingValueException("Todo Item Text");

                TodoItem newItem = new()
                {
                    TodoListId = todoList.Id,
                    Text = createDto.Text,
                    UpdatedAt = DateTime.UtcNow
                };

                var created = await todoItemRepository.CreateTodoItemAsync(newItem);
                createdItems.Add(created.ToDto());
            }
        }

        // Process updates
        if (batchUpdateDto.Updates != null)
        {
            foreach (var updateDto in batchUpdateDto.Updates)
            {
                if (updateDto.Id == Guid.Empty)
                    throw new MissingValueException("Todo Item Id");

                if (string.IsNullOrWhiteSpace(updateDto.Text))
                    throw new MissingValueException("Todo Item Text");

                TodoItem todoItem = await todoItemRepository.GetTodoItemByIdAsync(updateDto.Id)
                    ?? throw new NotFoundException("Todo Item", "Id", updateDto.Id);

                if (todoItem.TodoListId != todoList.Id)
                    throw new ForbiddenException("Todo Item does not belong to the specified list");

                todoItem.Text = updateDto.Text;
                todoItem.UpdatedAt = DateTime.UtcNow;
                todoItemRepository.UpdateTodoItem(todoItem);
                updatedItems.Add(todoItem.ToDto());
            }
        }

        // Process completes
        if (batchUpdateDto.Completes != null)
        {
            foreach (var itemId in batchUpdateDto.Completes)
            {
                if (itemId == Guid.Empty)
                    throw new MissingValueException("Todo Item Id");

                TodoItem todoItem = await todoItemRepository.GetTodoItemByIdAsync(itemId)
                    ?? throw new NotFoundException("Todo Item", "Id", itemId);

                if (todoItem.TodoListId != todoList.Id)
                    throw new ForbiddenException("Todo Item does not belong to the specified list");

                if (todoItem.IsCompleted)
                    throw new BusinessLogicException($"TodoItem {itemId} is already completed");

                todoItemRepository.MarkTodoItemAsComplete(todoItem);
                completedItems.Add(todoItem.ToDto());
            }
        }

        // Process incompletes
        if (batchUpdateDto.Incompletes != null)
        {
            foreach (var itemId in batchUpdateDto.Incompletes)
            {
                if (itemId == Guid.Empty)
                    throw new MissingValueException("Todo Item Id");

                TodoItem todoItem = await todoItemRepository.GetTodoItemByIdAsync(itemId)
                    ?? throw new NotFoundException("Todo Item", "Id", itemId);

                if (todoItem.TodoListId != todoList.Id)
                    throw new ForbiddenException("Todo Item does not belong to the specified list");

                if (!todoItem.IsCompleted)
                    throw new BusinessLogicException($"TodoItem {itemId} is already incomplete");

                todoItemRepository.MarkTodoItemAsIncomplete(todoItem);
                incompletedItems.Add(todoItem.ToDto());
            }
        }

        // Process deletes
        if (batchUpdateDto.Deletes != null)
        {
            foreach (var itemId in batchUpdateDto.Deletes)
            {
                if (itemId == Guid.Empty)
                    throw new MissingValueException("Todo Item Id");

                TodoItem todoItem = await todoItemRepository.GetTodoItemByIdAsync(itemId)
                    ?? throw new NotFoundException("Todo Item", "Id", itemId);

                if (todoItem.TodoListId != todoList.Id)
                    throw new ForbiddenException("Todo Item does not belong to the specified list");

                todoItemRepository.DeleteTodoItem(todoItem);
                deletedIds.Add(itemId);
            }
        }

        // Save all changes in a single transaction
        await todoItemRepository.SaveChangesAsync();

        return new BatchUpdateTodoItemsResultDto(
            Created: createdItems,
            Updated: updatedItems,
            Completed: completedItems,
            Incompleted: incompletedItems,
            Deleted: deletedIds
        );
    }
}