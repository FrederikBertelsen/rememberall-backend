namespace RememberAll.src.Services.Interfaces;

public interface IUserService
{
    // Task<UserDto> GetUserByIdAsync(Guid userId);
    // public Task<UserDto> GetUserWithRelationsByIdAsync(Guid userId);
    // public Task<UserDto> GetUserByEmailAsync(string email);
    // public Task<bool> UserExistsByIdAsync(Guid userId);
    // public Task<bool> UserExistsByEmailAsync(string email);
    public Task DeleteUserByIdAsync();
}