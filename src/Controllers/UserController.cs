using RememberAll.src.DTOs;
using RememberAll.src.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace RememberAll.src.Controllers;

[Route("api/users")]
[ApiController]
public class UserController(IUserService userService) : ControllerBase
{
    [HttpGet("by-id")]
    public async Task<ActionResult<UserDto>> GetUserById(Guid userId)
    {
        var userDto = await userService.GetUserByIdAsync(userId);
        return Ok(userDto);
    }

    [HttpGet("by-email")]
    public async Task<ActionResult<UserDto>> GetUserByEmail(string email)
    {
        var userDto = await userService.GetUserByEmailAsync(email);
        return Ok(userDto);
    }

    [HttpGet("by-id-with-relations")]
    public async Task<ActionResult<UserDto>> GetUserWithRelationsById(Guid userId)
    {
        var userDto = await userService.GetUserWithRelationsByIdAsync(userId);
        return Ok(userDto);
    }

    [HttpGet("exists-by-id")]
    public async Task<ActionResult<bool>> UserExistsById(Guid userId)
    {
        var exists = await userService.UserExistsByIdAsync(userId);
        return Ok(exists);
    }
    [HttpGet("exists-by-email")]
    public async Task<ActionResult<bool>> UserExistsByEmail(string email)
    {
        var exists = await userService.UserExistsByEmailAsync(email);
        return Ok(exists);
    }

    [HttpDelete("delete")]
    public async Task<IActionResult> DeleteUser(Guid userId)
    {
        await userService.DeleteUserByIdAsync(userId);
        return NoContent();
    }
}