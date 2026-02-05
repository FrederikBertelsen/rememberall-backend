namespace RememberAll.src.DTOs.Create;

public record CreateInviteDto
(
    string InviteRecieverEmail,
    Guid ListId
);