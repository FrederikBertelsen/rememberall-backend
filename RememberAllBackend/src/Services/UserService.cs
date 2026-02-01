using RememberAll.src.DTOs;
using RememberAll.src.Entities;
using RememberAll.src.Exceptions;
using RememberAll.src.Extensions;
using RememberAll.src.Repositories.Interfaces;
using RememberAll.src.Services.Interfaces;

namespace RememberAll.src.Services;

public class UserService(
    IUserRepository userRepository,
    ICurrentUserService currentUserService) : IUserService
{
    // public async Task<UserDto> GetUserByIdAsync(Guid userId)
    // {
    //     if (userId == Guid.Empty)
    //         throw new MissingValueException("User Id");

    //     User foundUser = await userRepository.GetUserByIdAsync(userId)
    //         ?? throw new NotFoundException("User", "Id", userId);

    //     return foundUser.ToDto();
    // }


    // public async Task<UserDto> GetUserByEmailAsync(string email)
    // {
    //     if (string.IsNullOrWhiteSpace(email))
    //         throw new MissingValueException("User Email");

    //     User foundUser = await userRepository.GetUserByEmailAsync(email)
    //         ?? throw new NotFoundException("User", "Email", email);

    //     return foundUser.ToDto();
    // }
    // public async Task<UserDto> GetUserWithRelationsByIdAsync(Guid userId)
    // {
    //     if (userId == Guid.Empty)
    //         throw new MissingValueException("User Id");

    //     User foundUser = await userRepository.GetUserWithRelationsByIdAsync(userId)
    //         ?? throw new NotFoundException("User", "Id", userId);

    //     return foundUser.ToDto();
    // }

    // public async Task<bool> UserExistsByIdAsync(Guid userId)
    // {
    //     if (userId == Guid.Empty)
    //         throw new MissingValueException("User Id");

    //     return await userRepository.UserExistsByIdAsync(userId);
    // }

    // public async Task<bool> UserExistsByEmailAsync(string email)
    // {
    //     if (string.IsNullOrWhiteSpace(email))
    //         throw new MissingValueException("User Email");

    //     return await userRepository.UserExistsByEmailAsync(email);
    // }

    public async Task DeleteUserByIdAsync()
    {
        var userId = currentUserService.GetUserId();

        User userToDelete = await userRepository.GetUserByIdAsync(userId)
            ?? throw new NotFoundException("User", "Id", userId);

        userRepository.DeleteUser(userToDelete);

        await userRepository.SaveChangesAsync();
    }
}