namespace RememberAll.src.DTOs.Create;

public record CreateTodoItemDto
(
    Guid TodoListId,
    string Text
);