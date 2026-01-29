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

    #region TodoList Validations
    public static void ValidateOrThrow(this CreateTodoListDto createTodoListDto)
    {
        if (createTodoListDto is null)
            throw new MissingValueException("TodoList data");
        if (createTodoListDto.OwnerId == Guid.Empty)
            throw new MissingValueException("TodoList", nameof(createTodoListDto.OwnerId));
        if (string.IsNullOrWhiteSpace(createTodoListDto.Name))
            throw new MissingValueException("TodoList", nameof(createTodoListDto.Name));
    }
    public static void ValidateOrThrow(this TodoListDto todoListDto)
    {
        if (todoListDto is null)
            throw new MissingValueException("TodoList data");
        if (todoListDto.OwnerId == Guid.Empty)
            throw new MissingValueException("TodoList", nameof(todoListDto.OwnerId));
        if (string.IsNullOrWhiteSpace(todoListDto.Name))
            throw new MissingValueException("TodoList", nameof(todoListDto.Name));
        if (todoListDto.Items is null)
            throw new MissingValueException("TodoList", nameof(todoListDto.Items));
    }
    #endregion
}