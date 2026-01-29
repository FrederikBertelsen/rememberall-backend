namespace RememberAll.src.DTOs;

public record TodoItemDto
(
    Guid Id,
    string Text,
    bool IsCompleted,
    int CompletionCount
);