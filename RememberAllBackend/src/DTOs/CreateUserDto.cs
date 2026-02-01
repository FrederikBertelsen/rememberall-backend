namespace RememberAll.src.DTOs;

public record CreateUserDto
(
    string Name,
    string Email,
    string Password
);