using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RememberAll.src.DTOs;
using RememberAll.src.DTOs.Create;
using RememberAll.src.Services.Interfaces;

namespace RememberAll.src.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{

    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(CreateUserDto createUserDto)
    {
        UserDto newUser = await authService.Register(createUserDto);
        return Ok(newUser);
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        UserDto user = await authService.Login(loginDto);
        return Ok(user);
    }

    [HttpGet("password-requirements")]
    public ActionResult<string> GetPasswordRequirements()
    {
        string passwordRequirements = authService.GetPasswordRequirements();
        return Ok(passwordRequirements);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        await authService.Logout();
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me()
    {
        UserDto? user = await authService.Me();
        return Ok(user);
    }

    [Authorize]
    [HttpDelete("delete-account")]
    public async Task<ActionResult> DeleteAccount()
    {
        await authService.DeleteAccount();
        return NoContent();
    }
}
