using RememberAll.src.DTOs;
using RememberAll.src.DTOs.Create;
using RememberAll.src.DTOs.Update;
using RememberAll.src.Exceptions;
using RememberAll.src.Utilities;

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

        if (string.IsNullOrWhiteSpace(createUserDto.Password))
            throw new MissingValueException("User", nameof(createUserDto.Password));

        PasswordValidationResult results = PasswordValidator.Validate(createUserDto.Password);
        if (!results.IsValid)
            throw new InvalidValueException("User", nameof(createUserDto.Password), results.ValidationErrors);
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

    #region TodoItem Validations
    public static void ValidateOrThrow(this CreateTodoItemDto createTodoItemDto)
    {
        if (createTodoItemDto is null)
            throw new MissingValueException("TodoItem data");
        if (createTodoItemDto.TodoListId == Guid.Empty)
            throw new MissingValueException("TodoItem", nameof(createTodoItemDto.TodoListId));
        if (string.IsNullOrWhiteSpace(createTodoItemDto.Text))
            throw new MissingValueException("TodoItem", nameof(createTodoItemDto.Text));
    }
    public static void ValidateOrThrow(this UpdateTodoItemDto updateTodoItemDto)
    {
        if (updateTodoItemDto is null)
            throw new MissingValueException("TodoItem data");
        if (updateTodoItemDto.Id == Guid.Empty)
            throw new MissingValueException("TodoItem", nameof(updateTodoItemDto.Id));
        if (updateTodoItemDto.Text is not null && string.IsNullOrWhiteSpace(updateTodoItemDto.Text))
            throw new MissingValueException("TodoItem", nameof(updateTodoItemDto.Text));
    }
    public static void ValidateOrThrow(this TodoItemDto todoItemDto)
    {
        if (todoItemDto is null)
            throw new MissingValueException("TodoItem data");
        if (string.IsNullOrWhiteSpace(todoItemDto.Text))
            throw new MissingValueException("TodoItem", nameof(todoItemDto.Text));
    }
    #endregion

    #region ListAccess Validations
    public static void ValidateOrThrow(this CreateListAccessDto createListAccessDto)
    {
        if (createListAccessDto is null)
            throw new MissingValueException("ListAccess data");
        if (createListAccessDto.UserId == Guid.Empty)
            throw new MissingValueException("ListAccess", nameof(createListAccessDto.UserId));
        if (createListAccessDto.ListId == Guid.Empty)
            throw new MissingValueException("ListAccess", nameof(createListAccessDto.ListId));
    }

    public static void ValidateOrThrow(this ListAccessDto listAccessDto)
    {
        if (listAccessDto is null)
            throw new MissingValueException("ListAccess data");
        if (listAccessDto.Id == Guid.Empty)
            throw new MissingValueException("ListAccess", nameof(listAccessDto.Id));
        if (listAccessDto.UserId == Guid.Empty)
            throw new MissingValueException("ListAccess", nameof(listAccessDto.UserId));
        if (string.IsNullOrWhiteSpace(listAccessDto.UserName))
            throw new MissingValueException("ListAccess", nameof(listAccessDto.UserName));
        if (listAccessDto.ListId == Guid.Empty)
            throw new MissingValueException("ListAccess", nameof(listAccessDto.ListId));
        if (string.IsNullOrWhiteSpace(listAccessDto.ListName))
            throw new MissingValueException("ListAccess", nameof(listAccessDto.ListName));
    }
    #endregion

    #region Invite Validations
    public static void ValidateOrThrow(this CreateInviteDto createInviteDto)
    {
        if (createInviteDto is null)
            throw new MissingValueException("Invite data");
        if (string.IsNullOrWhiteSpace(createInviteDto.InviteRecieverEmail))
            throw new MissingValueException("Invite", nameof(createInviteDto.InviteRecieverEmail));
        if (createInviteDto.ListId == Guid.Empty)
            throw new MissingValueException("Invite", nameof(createInviteDto.ListId));
    }
    #endregion

    #region Auth Validations
    public static void ValidateOrThrow(this LoginDto loginDto)
    {
        if (loginDto is null)
            throw new MissingValueException("Login data");
        if (string.IsNullOrWhiteSpace(loginDto.Email))
            throw new MissingValueException("User", nameof(loginDto.Email));
        if (!EmailValidator.Validate(loginDto.Email))
            throw new InvalidValueException("User", nameof(loginDto.Email), "Invalid email format");
        if (string.IsNullOrWhiteSpace(loginDto.Password))
            throw new MissingValueException("User", nameof(loginDto.Password));
    }
    #endregion
}