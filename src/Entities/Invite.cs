namespace rememberall.src.Entities;

public class Invite : BaseEntity
{
    public required Guid InviterId { get; init; }
    public required User Inviter { get; init; }

    public required Guid InviteeId { get; init; }
    public required User Invitee { get; init; }
    
    public required Guid ListId { get; init; }
    public required TodoList List { get; init; }
}