namespace RememberAll.src.DTOs;

public record TodoListDto
(
    Guid Id,
    Guid OwnerId,
    string Name,
    ICollection<TodoItemDto> Items
);