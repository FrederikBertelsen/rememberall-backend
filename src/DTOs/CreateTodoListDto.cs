namespace RememberAll.src.DTOs;

public record CreateTodoListDto
(
    Guid OwnerId,
    string Name
);