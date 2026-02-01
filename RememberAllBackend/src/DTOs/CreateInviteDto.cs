namespace RememberAll.src.DTOs;

public record CreateInviteDto
(
    Guid InviteRecieverId,
    Guid ListId
);