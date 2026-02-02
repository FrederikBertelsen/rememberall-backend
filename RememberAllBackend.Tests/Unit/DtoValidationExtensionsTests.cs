using FluentAssertions;
using RememberAll.src.DTOs;
using RememberAll.src.DTOs.Create;
using RememberAll.src.DTOs.Update;
using RememberAll.src.Exceptions;
using RememberAll.src.Extensions;

#pragma warning disable CS8604 // Possible null reference argument - testing null scenarios

namespace RememberAllBackend.Tests.Unit;

public class DtoValidationExtensionsTests
{
    #region CreateUserDto Tests

    [Fact]
    public void ValidateOrThrow_CreateUserDto_WithValidData_DoesNotThrow()
    {
        // Arrange
        var dto = new CreateUserDto("John", "john@example.com", "Pass123!@#");

        // Act & Assert
        dto.Invoking(d => d.ValidateOrThrow()).Should().NotThrow();
    }

    [Fact]
    public void ValidateOrThrow_CreateUserDto_WhenNull_ThrowsMissingValueException()
    {
        // Arrange
        CreateUserDto? dto = null;

        // Act & Assert
        Assert.Throws<MissingValueException>(() => dto.ValidateOrThrow());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateOrThrow_CreateUserDto_WithInvalidName_ThrowsMissingValueException(string? invalidName)
    {
        // Arrange
        var dto = new CreateUserDto(invalidName!, "john@example.com", "Pass123!@#");

        // Act & Assert
        dto.Invoking(d => d.ValidateOrThrow()).Should().Throw<MissingValueException>()
            .WithMessage("*Name*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateOrThrow_CreateUserDto_WithInvalidEmail_ThrowsMissingValueException(string? invalidEmail)
    {
        // Arrange
        var dto = new CreateUserDto("John", invalidEmail!, "Pass123!@#");

        // Act & Assert
        dto.Invoking(d => d.ValidateOrThrow()).Should().Throw<MissingValueException>()
            .WithMessage("*Email*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateOrThrow_CreateUserDto_WithInvalidPassword_ThrowsMissingValueException(string? invalidPassword)
    {
        // Arrange
        var dto = new CreateUserDto("John", "john@example.com", invalidPassword!);

        // Act & Assert
        dto.Invoking(d => d.ValidateOrThrow()).Should().Throw<MissingValueException>()
            .WithMessage("*Password*");
    }

    [Fact]
    public void ValidateOrThrow_CreateUserDto_WithWeakPassword_ThrowsInvalidValueException()
    {
        // Arrange
        var dto = new CreateUserDto("John", "john@example.com", "weak");

        // Act & Assert
        dto.Invoking(d => d.ValidateOrThrow()).Should().Throw<InvalidValueException>()
            .WithMessage("*Password*");
    }

    #endregion

    #region UserDto Tests

    [Fact]
    public void ValidateOrThrow_UserDto_WithValidData_DoesNotThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var dto = new UserDto(userId, "John", "john@example.com");

        // Act & Assert
        dto.Invoking(d => d.ValidateOrThrow()).Should().NotThrow();
    }

    [Fact]
    public void ValidateOrThrow_UserDto_WhenNull_ThrowsMissingValueException()
    {
        // Arrange
        UserDto? dto = null;

        // Act & Assert
        Assert.Throws<MissingValueException>(() => dto.ValidateOrThrow());
    }

    [Fact]
    public void ValidateOrThrow_UserDto_WithEmptyId_ThrowsMissingValueException()
    {
        // Arrange
        var dto = new UserDto(Guid.Empty, "John", "john@example.com");

        // Act & Assert
        dto.Invoking(d => d.ValidateOrThrow()).Should().Throw<MissingValueException>()
            .WithMessage("*Id*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateOrThrow_UserDto_WithInvalidName_ThrowsMissingValueException(string? invalidName)
    {
        // Arrange
        var dto = new UserDto(Guid.NewGuid(), invalidName, "john@example.com");

        // Act & Assert
        dto.Invoking(d => d.ValidateOrThrow()).Should().Throw<MissingValueException>()
            .WithMessage("*Name*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateOrThrow_UserDto_WithInvalidEmail_ThrowsMissingValueException(string? invalidEmail)
    {
        // Arrange
        var dto = new UserDto(Guid.NewGuid(), "John", invalidEmail!);

        // Act & Assert
        dto.Invoking(d => d.ValidateOrThrow()).Should().Throw<MissingValueException>()
            .WithMessage("*Email*");
    }

    #endregion

    #region CreateTodoListDto Tests

    [Fact]
    public void ValidateOrThrow_CreateTodoListDto_WithValidData_DoesNotThrow()
    {
        // Arrange
        var dto = new CreateTodoListDto("Shopping");

        // Act & Assert
        dto.Invoking(d => d.ValidateOrThrow()).Should().NotThrow();
    }

    [Fact]
    public void ValidateOrThrow_CreateTodoListDto_WhenNull_ThrowsMissingValueException()
    {
        // Arrange
        CreateTodoListDto? dto = null;

        // Act & Assert
        Assert.Throws<MissingValueException>(() => dto.ValidateOrThrow());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateOrThrow_CreateTodoListDto_WithInvalidName_ThrowsMissingValueException(string? invalidName)
    {
        // Arrange
        var dto = new CreateTodoListDto(invalidName!);

        // Act & Assert
        dto.Invoking(d => d.ValidateOrThrow()).Should().Throw<MissingValueException>()
            .WithMessage("*Name*");
    }

    #endregion

    #region TodoListDto Tests

    [Fact]
    public void ValidateOrThrow_TodoListDto_WithValidData_DoesNotThrow()
    {
        // Arrange
        var dto = new TodoListDto(Guid.NewGuid(), Guid.NewGuid(), "Shopping", []);

        // Act & Assert
        dto.Invoking(d => d.ValidateOrThrow()).Should().NotThrow();
    }

    [Fact]
    public void ValidateOrThrow_TodoListDto_WhenNull_ThrowsMissingValueException()
    {
        // Arrange
        TodoListDto? dto = null;

        // Act & Assert
        Assert.Throws<MissingValueException>(() => dto.ValidateOrThrow());
    }

    [Fact]
    public void ValidateOrThrow_TodoListDto_WithEmptyOwnerId_ThrowsMissingValueException()
    {
        // Arrange
        var dto = new TodoListDto(Guid.NewGuid(), Guid.Empty, "Shopping", []);

        // Act & Assert
        dto.Invoking(d => d.ValidateOrThrow()).Should().Throw<MissingValueException>()
            .WithMessage("*OwnerId*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateOrThrow_TodoListDto_WithInvalidName_ThrowsMissingValueException(string? invalidName)
    {
        // Arrange
        var dto = new TodoListDto(Guid.NewGuid(), Guid.NewGuid(), invalidName!, []);

        // Act & Assert
        dto.Invoking(d => d.ValidateOrThrow()).Should().Throw<MissingValueException>()
            .WithMessage("*Name*");
    }

    [Fact]
    public void ValidateOrThrow_TodoListDto_WithNullItems_ThrowsMissingValueException()
    {
        // Arrange
        var dto = new TodoListDto(Guid.NewGuid(), Guid.NewGuid(), "Shopping", null!);

        // Act & Assert
        dto.Invoking(d => d.ValidateOrThrow()).Should().Throw<MissingValueException>()
            .WithMessage("*Items*");
    }

    #endregion

    #region CreateTodoItemDto Tests

    [Fact]
    public void ValidateOrThrow_CreateTodoItemDto_WithValidData_DoesNotThrow()
    {
        // Arrange
        var dto = new CreateTodoItemDto(Guid.NewGuid(), "Buy milk");

        // Act & Assert
        dto.Invoking(d => d.ValidateOrThrow()).Should().NotThrow();
    }

    [Fact]
    public void ValidateOrThrow_CreateTodoItemDto_WhenNull_ThrowsMissingValueException()
    {
        // Arrange
        CreateTodoItemDto? dto = null;

        // Act & Assert
        Assert.Throws<MissingValueException>(() => dto.ValidateOrThrow());
    }

    [Fact]
    public void ValidateOrThrow_CreateTodoItemDto_WithEmptyTodoListId_ThrowsMissingValueException()
    {
        // Arrange
        var dto = new CreateTodoItemDto(Guid.Empty, "Buy milk");

        // Act & Assert
        dto.Invoking(d => d.ValidateOrThrow()).Should().Throw<MissingValueException>()
            .WithMessage("*TodoListId*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateOrThrow_CreateTodoItemDto_WithInvalidText_ThrowsMissingValueException(string? invalidText)
    {
        // Arrange
        var dto = new CreateTodoItemDto(Guid.NewGuid(), invalidText!);

        // Act & Assert
        dto.Invoking(d => d.ValidateOrThrow()).Should().Throw<MissingValueException>()
            .WithMessage("*Text*");
    }

    #endregion

    #region UpdateTodoItemDto Tests

    [Fact]
    public void ValidateOrThrow_UpdateTodoItemDto_WithValidData_DoesNotThrow()
    {
        // Arrange
        var dto = new UpdateTodoItemDto(Guid.NewGuid(), "Updated text");

        // Act & Assert
        dto.Invoking(d => d.ValidateOrThrow()).Should().NotThrow();
    }

    [Fact]
    public void ValidateOrThrow_UpdateTodoItemDto_WhenNull_ThrowsMissingValueException()
    {
        // Arrange
        UpdateTodoItemDto? dto = null;

        // Act & Assert
        Assert.Throws<MissingValueException>(() => dto.ValidateOrThrow());
    }

    [Fact]
    public void ValidateOrThrow_UpdateTodoItemDto_WithEmptyId_ThrowsMissingValueException()
    {
        // Arrange
        var dto = new UpdateTodoItemDto(Guid.Empty, "Updated text");

        // Act & Assert
        dto.Invoking(d => d.ValidateOrThrow()).Should().Throw<MissingValueException>()
            .WithMessage("*Id*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateOrThrow_UpdateTodoItemDto_WithInvalidText_ThrowsMissingValueException(string invalidText)
    {
        // Arrange
        var dto = new UpdateTodoItemDto(Guid.NewGuid(), invalidText);

        // Act & Assert
        dto.Invoking(d => d.ValidateOrThrow()).Should().Throw<MissingValueException>()
            .WithMessage("*Text*");
    }

    [Fact]
    public void ValidateOrThrow_UpdateTodoItemDto_WithNullText_DoesNotThrow()
    {
        // Arrange - null text means don't update it
        var dto = new UpdateTodoItemDto(Guid.NewGuid(), null!);

        // Act & Assert
        dto.Invoking(d => d.ValidateOrThrow()).Should().NotThrow();
    }

    #endregion

    #region TodoItemDto Tests

    [Fact]
    public void ValidateOrThrow_TodoItemDto_WithValidData_DoesNotThrow()
    {
        // Arrange
        var dto = new TodoItemDto(Guid.NewGuid(), "Buy milk", false, 0);

        // Act & Assert
        dto.Invoking(d => d.ValidateOrThrow()).Should().NotThrow();
    }

    [Fact]
    public void ValidateOrThrow_TodoItemDto_WhenNull_ThrowsMissingValueException()
    {
        // Arrange
        TodoItemDto? dto = null;

        // Act & Assert
        Assert.Throws<MissingValueException>(() => dto.ValidateOrThrow());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateOrThrow_TodoItemDto_WithInvalidText_ThrowsMissingValueException(string? invalidText)
    {
        // Arrange
        var dto = new TodoItemDto(Guid.NewGuid(), invalidText!, false, 0);

        // Act & Assert
        dto.Invoking(d => d.ValidateOrThrow()).Should().Throw<MissingValueException>()
            .WithMessage("*Text*");
    }

    #endregion

    #region CreateListAccessDto Tests

    [Fact]
    public void ValidateOrThrow_CreateListAccessDto_WithValidData_DoesNotThrow()
    {
        // Arrange
        var dto = new CreateListAccessDto(Guid.NewGuid(), Guid.NewGuid());

        // Act & Assert
        dto.Invoking(d => d.ValidateOrThrow()).Should().NotThrow();
    }

    [Fact]
    public void ValidateOrThrow_CreateListAccessDto_WhenNull_ThrowsMissingValueException()
    {
        // Arrange
        CreateListAccessDto? dto = null;

        // Act & Assert
        Assert.Throws<MissingValueException>(() => dto.ValidateOrThrow());
    }

    [Fact]
    public void ValidateOrThrow_CreateListAccessDto_WithEmptyUserId_ThrowsMissingValueException()
    {
        // Arrange
        var dto = new CreateListAccessDto(Guid.Empty, Guid.NewGuid());

        // Act & Assert
        dto.Invoking(d => d.ValidateOrThrow()).Should().Throw<MissingValueException>()
            .WithMessage("*UserId*");
    }

    [Fact]
    public void ValidateOrThrow_CreateListAccessDto_WithEmptyListId_ThrowsMissingValueException()
    {
        // Arrange
        var dto = new CreateListAccessDto(Guid.NewGuid(), Guid.Empty);

        // Act & Assert
        dto.Invoking(d => d.ValidateOrThrow()).Should().Throw<MissingValueException>()
            .WithMessage("*ListId*");
    }

    #endregion

    #region ListAccessDto Tests

    [Fact]
    public void ValidateOrThrow_ListAccessDto_WithValidData_DoesNotThrow()
    {
        // Arrange
        var dto = new ListAccessDto(Guid.NewGuid(), Guid.NewGuid(), "John", Guid.NewGuid(), "Shopping");

        // Act & Assert
        dto.Invoking(d => d.ValidateOrThrow()).Should().NotThrow();
    }

    [Fact]
    public void ValidateOrThrow_ListAccessDto_WhenNull_ThrowsMissingValueException()
    {
        // Arrange
        ListAccessDto? dto = null;

        // Act & Assert
        Assert.Throws<MissingValueException>(() => dto.ValidateOrThrow());
    }

    [Fact]
    public void ValidateOrThrow_ListAccessDto_WithEmptyId_ThrowsMissingValueException()
    {
        // Arrange
        var dto = new ListAccessDto(Guid.Empty, Guid.NewGuid(), "John", Guid.NewGuid(), "Shopping");

        // Act & Assert
        dto.Invoking(d => d.ValidateOrThrow()).Should().Throw<MissingValueException>()
            .WithMessage("*Id*");
    }

    [Fact]
    public void ValidateOrThrow_ListAccessDto_WithEmptyUserId_ThrowsMissingValueException()
    {
        // Arrange
        var dto = new ListAccessDto(Guid.NewGuid(), Guid.Empty, "John", Guid.NewGuid(), "Shopping");

        // Act & Assert
        dto.Invoking(d => d.ValidateOrThrow()).Should().Throw<MissingValueException>()
            .WithMessage("*UserId*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateOrThrow_ListAccessDto_WithInvalidUserName_ThrowsMissingValueException(string? invalidUserName)
    {
        // Arrange
        var dto = new ListAccessDto(Guid.NewGuid(), Guid.NewGuid(), invalidUserName!, Guid.NewGuid(), "Shopping");

        // Act & Assert
        dto.Invoking(d => d.ValidateOrThrow()).Should().Throw<MissingValueException>()
            .WithMessage("*UserName*");
    }

    [Fact]
    public void ValidateOrThrow_ListAccessDto_WithEmptyListId_ThrowsMissingValueException()
    {
        // Arrange
        var dto = new ListAccessDto(Guid.NewGuid(), Guid.NewGuid(), "John", Guid.Empty, "Shopping");

        // Act & Assert
        dto.Invoking(d => d.ValidateOrThrow()).Should().Throw<MissingValueException>()
            .WithMessage("*ListId*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateOrThrow_ListAccessDto_WithInvalidListName_ThrowsMissingValueException(string? invalidListName)
    {
        // Arrange
        var dto = new ListAccessDto(Guid.NewGuid(), Guid.NewGuid(), "John", Guid.NewGuid(), invalidListName!);

        // Act & Assert
        dto.Invoking(d => d.ValidateOrThrow()).Should().Throw<MissingValueException>()
            .WithMessage("*ListName*");
    }

    #endregion

    #region CreateInviteDto Tests

    [Fact]
    public void ValidateOrThrow_CreateInviteDto_WithValidData_DoesNotThrow()
    {
        // Arrange
        var dto = new CreateInviteDto(Guid.NewGuid(), Guid.NewGuid());

        // Act & Assert
        dto.Invoking(d => d.ValidateOrThrow()).Should().NotThrow();
    }

    [Fact]
    public void ValidateOrThrow_CreateInviteDto_WhenNull_ThrowsMissingValueException()
    {
        // Arrange
        CreateInviteDto? dto = null;

        // Act & Assert
        Assert.Throws<MissingValueException>(() => dto.ValidateOrThrow());
    }

    [Fact]
    public void ValidateOrThrow_CreateInviteDto_WithEmptyInviteRecieverId_ThrowsMissingValueException()
    {
        // Arrange
        var dto = new CreateInviteDto(Guid.Empty, Guid.NewGuid());

        // Act & Assert
        dto.Invoking(d => d.ValidateOrThrow()).Should().Throw<MissingValueException>()
            .WithMessage("*InviteRecieverId*");
    }

    [Fact]
    public void ValidateOrThrow_CreateInviteDto_WithEmptyListId_ThrowsMissingValueException()
    {
        // Arrange
        var dto = new CreateInviteDto(Guid.NewGuid(), Guid.Empty);

        // Act & Assert
        dto.Invoking(d => d.ValidateOrThrow()).Should().Throw<MissingValueException>()
            .WithMessage("*ListId*");
    }

    #endregion
}
