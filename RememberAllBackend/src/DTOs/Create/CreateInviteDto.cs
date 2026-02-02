namespace RememberAll.src.DTOs.Create;

public record CreateInviteDto
(
    Guid InviteRecieverId,
    Guid ListId
);