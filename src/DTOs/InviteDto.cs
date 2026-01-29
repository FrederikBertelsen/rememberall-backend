namespace RememberAll.src.DTOs;

public record InviteDto
(
    Guid Id,
    Guid InviteSenderId,
    string InviteSenderName,
    Guid InviteRecieverId,
    string InviteRecieverName,
    Guid ListId,
    string ListName
);