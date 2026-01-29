namespace rememberall.src.Entities;

public class ListAccess : BaseEntity
{
    public required Guid UserId { get; init; }
    public required User User { get; init; }
    public required Guid ListId { get; init; }
    public required TodoList List { get; init; }

}