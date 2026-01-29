using Microsoft.EntityFrameworkCore;
using RememberAll.src.Data;
using RememberAll.src.Entities;
using RememberAll.src.Repositories.Interfaces;

namespace RememberAll.src.Repositories;

public class UserRepository(AppDbContext dbContext) : IUserRepository
{
    public async Task<User> CreateUserAsync(User user) => (await dbContext.Users.AddAsync(user)).Entity;

    public async Task<User?> GetUserByIdAsync(Guid userId) => await dbContext.Users.FirstOrDefaultAsync(user => user.Id == userId);
    public async Task<User?> GetUserWithRelationsByIdAsync(Guid userId) =>
        await dbContext.Users
        .Include(user => user.ListAccess)
        .Include(user => user.Lists)
        .FirstOrDefaultAsync(user => user.Id == userId);
    public async Task<User?> GetUserByEmailAsync(string email) => await dbContext.Users.FirstOrDefaultAsync(user => user.Email == email);

    public async Task<bool> UserExistsByIdAsync(Guid userId) => await dbContext.Users.AnyAsync(user => user.Id == userId);
    public async Task<bool> UserExistsByEmailAsync(string email) => await dbContext.Users.AnyAsync(user => user.Email == email);

    public void DeleteUser(User user) => dbContext.Users.Remove(user);

    public async Task SaveChangesAsync() => await dbContext.SaveChangesAsync();
}