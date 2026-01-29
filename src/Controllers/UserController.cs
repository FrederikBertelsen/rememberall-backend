using RememberAll.src.DTOs;
using RememberAll.src.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace RememberAll.src.Controllers;

[Route("api/users")]
[ApiController]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpPost("create")]
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto newUserDto)
    {
        var createdUserDto = await userService.CreateUserAsync(newUserDto);
        return Ok(createdUserDto);
    }

    [HttpGet("byid")]
    public async Task<ActionResult<UserDto>> GetUserById(Guid userId)
    {
        var userDto = await userService.GetUserByIdAsync(userId);
        return Ok(userDto);
    }

        
    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        await userService.DeleteUserByIdAsync(userId);
        return NoContent();
    }
}
