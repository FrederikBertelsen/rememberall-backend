namespace RememberAll.src.DTOs;

public record BatchUpdateTodoItemsResultDto
(
    ICollection<TodoItemDto> Created,
    ICollection<TodoItemDto> Updated,
    ICollection<TodoItemDto> Completed,
    ICollection<TodoItemDto> Incompleted,
    ICollection<Guid> Deleted
);
