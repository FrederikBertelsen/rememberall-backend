using rememberall.src.DTOs;
using rememberall.src.Entities;

namespace rememberall.src.Repositories.Interfaces;

public interface IUserRepository
{
    public Task<User> CreateUserAsync(User user);
    Task<User?> GetUserByIdAsync(Guid userId);
    public Task<User?> GetUserWithRelationsByIdAsync(Guid userId);
    public Task<User?> GetUserByEmailAsync(string email);
    public Task<bool> UserExistsByIdAsync(Guid userId);
    public Task<bool> UserExistsByEmailAsync(string email);
    public void DeleteUser(User user);
    
    public Task SaveChangesAsync();
}