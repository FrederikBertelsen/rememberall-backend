namespace RememberAll.src.Entities;

public class TodoItem : BaseEntity
{
    public required Guid TodoListId { get; set; }
    public TodoList? TodoList { get; set; }
    public required string Text { get; set; }
    public bool IsCompleted { get; private set; } = false;
    public int CompletionCount { get; private set; } = 0;


    public void MarkAsComplete()
    {
        if (IsCompleted)
            throw new InvalidOperationException("Item is already completed");

        IsCompleted = true;
        CompletionCount++;
    }

    public void MarkAsIncomplete()
    {
        if (!IsCompleted)
            throw new InvalidOperationException("Item is already incomplete");

        IsCompleted = false;
    }
}