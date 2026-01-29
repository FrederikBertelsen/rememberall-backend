namespace RememberAll.src.Entities;

public class TodoList : BaseEntity
{
    public required Guid OwnerId { get; init; }
    public required User Owner { get; init; }
    public required string Name { get; set; }
    public ICollection<TodoItem> Items { get; init; } = [];
    public ICollection<ListAccess> Accessors { get; set; } = [];
    public ICollection<Invite> Invites { get; set; } = [];
}