namespace RememberAll.src.DTOs;

public record CreateInviteDto
(
    Guid InviteSenderId,
    Guid InviteRecieverId,
    Guid ListId
);