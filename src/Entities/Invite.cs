namespace rememberall.src.Entities;

public class Invite : BaseEntity
{
    public required Guid InviteSenderId { get; init; }
    public required User InviterSender { get; init; }

    public required Guid InviteRecieverId { get; init; }
    public required User InviteReciever { get; init; }

    public required Guid ListId { get; init; }
    public required TodoList List { get; init; }
}