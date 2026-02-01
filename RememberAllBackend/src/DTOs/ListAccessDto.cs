namespace RememberAll.src.DTOs;

public record ListAccessDto
(
    Guid Id,
    Guid UserId,
    string UserName,
    Guid ListId,
    string ListName
);