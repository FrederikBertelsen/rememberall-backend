namespace rememberall.src.Entities;

public class Invite : BaseEntity
{
    public required Guid InviterId {get; init;}
    public required Guid InviteeId {get; init;}
    public required Guid ListId {get; init;}
}