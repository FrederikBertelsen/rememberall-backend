namespace RememberAll.src.DTOs;

public record TodoListDto
(
    Guid Id,
    Guid OwnerId,
    string Name,
    DateTime UpdatedAt,
    ICollection<TodoItemDto> Items
);