namespace rememberall.src.Entities;

public class User : BaseEntity
{
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required bool IsAdmin { get; init; } = false;
    public ICollection<TodoList> Lists { get; set; } = [];
    public ICollection<ListCollaborator> Collaborations { get; set; } = [];

}