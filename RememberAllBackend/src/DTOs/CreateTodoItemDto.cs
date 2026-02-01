namespace RememberAll.src.DTOs;

public record CreateTodoItemDto
(
    Guid TodoListId,
    string Text
);