using RememberAll.src.DTOs;
using RememberAll.src.Exceptions;

namespace RememberAll.src.Extensions;

public static class DtoValidationExtensions
{
    #region User Validations
    public static void ValidateOrThrow(this CreateUserDto createUserDto)
    {
        if (createUserDto is null)
            throw new MissingValueException("User data");
        if (string.IsNullOrWhiteSpace(createUserDto.Name))
            throw new MissingValueException("User", nameof(createUserDto.Name));
        if (string.IsNullOrWhiteSpace(createUserDto.Email))
            throw new MissingValueException("User", nameof(createUserDto.Email));
    }

    public static void ValidateOrThrow(this UserDto userDto)
    {
        if (userDto is null)
            throw new MissingValueException("User data");
        if (userDto.Id == Guid.Empty)
            throw new MissingValueException("User", nameof(userDto.Id));
        if (string.IsNullOrWhiteSpace(userDto.Name))
            throw new MissingValueException("User", nameof(userDto.Name));
        if (string.IsNullOrWhiteSpace(userDto.Email))
            throw new MissingValueException("User", nameof(userDto.Email));
    }
    #endregion
}