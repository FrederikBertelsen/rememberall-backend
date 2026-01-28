namespace rememberall.src.Entities;

public class Item : BaseEntity
{
    public required string Text { get; init; }
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