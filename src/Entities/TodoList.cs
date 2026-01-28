namespace rememberall.src.Entities;

public class TodoList : BaseEntity
{
    public required Guid OwnerId { get; init; }
    public required User Owner { get; init; }
    public required string Name { get; init; }
    public ICollection<TodoItem> Items { get; init; } = [];
    public ICollection<ListCollaborator> Collaborators { get; set; } = [];
    public ICollection<Invite> Invites { get; set; } = [];
}