namespace RememberAll.src.DTOs.Update;

public record BatchUpdateTodoItemsDto
(
    Guid TodoListId,
    ICollection<CreateBatchTodoItemDto>? Creates,
    ICollection<UpdateBatchTodoItemDto>? Updates,
    ICollection<Guid>? Completes,
    ICollection<Guid>? Incompletes,
    ICollection<Guid>? Deletes
);

public record CreateBatchTodoItemDto
(
    string Text
);

public record UpdateBatchTodoItemDto
(
    Guid Id,
    string Text
);
