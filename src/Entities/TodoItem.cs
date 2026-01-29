namespace RememberAll.src.Entities;

public class TodoItem : BaseEntity
{
    public required Guid TodoListId { get; init; }
    public required TodoList TodoList { get; init; }
    public required string Text { get; set; }
    public bool IsCompleted { get; private set; } = false;
    public int CompletionCount { get; private set; } = 0;


    public void MarkCompleted()
    {
        if (IsCompleted)
            throw new InvalidOperationException("Item is already completed");

        IsCompleted = true;
        CompletionCount++;
    }

    public void MarkIncomplete()
    {
        if (!IsCompleted)
            throw new InvalidOperationException("Item is already incomplete");

        IsCompleted = false;
    }
}