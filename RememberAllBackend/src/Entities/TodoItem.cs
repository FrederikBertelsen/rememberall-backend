namespace RememberAll.src.Entities;

public class TodoItem : BaseEntity
{
    public required Guid TodoListId { get; set; }
    public TodoList? TodoList { get; set; }
    public required string Text { get; set; }
    public bool IsCompleted { get; private set; } = false;
    public int CompletionCount { get; private set; } = 0;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    public void MarkAsComplete()
    {
        if (IsCompleted)
            throw new InvalidOperationException("Item is already completed");

        IsCompleted = true;
        CompletionCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsIncomplete()
    {
        if (!IsCompleted)
            throw new InvalidOperationException("Item is already incomplete");
        UpdatedAt = DateTime.UtcNow;

        IsCompleted = false;
    }
}