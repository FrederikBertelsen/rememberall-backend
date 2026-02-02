using System.Security.Claims;
using RememberAll.src.DTOs;
using RememberAll.src.DTOs.Create;

namespace RememberAll.src.Services.Interfaces;

public interface IAuthService
{
    public Task<UserDto> Register(CreateUserDto createUserDto);
    public Task<UserDto> Login(LoginDto loginDto);
    public Task Logout();
    public Task DeleteAccount();
    public Task<UserDto> Me();
    public string GetPasswordRequirements();
}