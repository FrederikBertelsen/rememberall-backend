namespace RememberAll.src.Entities;

public class ListAccess : BaseEntity
{
    public required Guid UserId { get; init; }
    public User? User { get; set; }
    public required Guid ListId { get; init; }
    public TodoList? List { get; set; }

}