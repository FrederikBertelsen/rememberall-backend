namespace RememberAll.src.DTOs.Create;

public record CreateUserDto
(
    string Name,
    string Email,
    string Password
);