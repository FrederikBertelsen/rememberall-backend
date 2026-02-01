using System.Security.Claims;
using RememberAll.src.DTOs;

namespace RememberAll.src.Services.Interfaces;

public interface IAuthService
{
    public Task<UserDto> Register(CreateUserDto createUserDto);
    public Task<UserDto> Login(LoginDto loginDto);
    public Task Logout();
    public Task<UserDto> Me();
    public string GetPasswordRequirements();
}