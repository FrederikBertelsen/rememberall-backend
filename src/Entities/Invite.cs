namespace RememberAll.src.Entities;

public class Invite : BaseEntity
{
    public required Guid InviteSenderId { get; init; }
    public User? InviteSender { get; set; }

    public required Guid InviteRecieverId { get; init; }
    public User? InviteReciever { get; set; }

    public required Guid ListId { get; init; }
    public TodoList? List { get; set; }
}