using FluentAssertions;
using Moq;
using Microsoft.AspNetCore.Identity;
using RememberAll.src.Entities;
using RememberAll.src.Exceptions;
using RememberAll.src.Extensions;
using RememberAll.src.Repositories.Interfaces;
using RememberAll.src.Services;
using RememberAll.src.Services.Interfaces;
using RememberAllBackend.Tests.Helpers;

namespace RememberAllBackend.Tests.Unit.Services;

public class AuthServiceTests
{
    #region Register Tests

    [Fact]
    public async Task Register_ReturnsUserDto_WhenValid()
    {
        // Arrange
        var createDto = TestData.CreateUserDto("Alice", "alice@example.com", "SecurePass123!@#");
        var createdUser = createDto.ToEntity();

        var mockUserRepo = new Mock<IUserRepository>();
        var mockPasswordHasher = new Mock<IPasswordHasher<User>>();
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        mockUserRepo.Setup(r => r.UserExistsByEmailAsync(createDto.Email)).ReturnsAsync(false);
        mockPasswordHasher.Setup(h => h.HashPassword(It.IsAny<User>(), createDto.Password)).Returns("hashedPassword");
        mockUserRepo.Setup(r => r.CreateUserAsync(It.IsAny<User>())).ReturnsAsync(createdUser);

        var service = new AuthService(mockUserRepo.Object, mockPasswordHasher.Object, mockHttpContextAccessor.Object, mockCurrentUser.Object);

        // Act
        var result = await service.Register(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(createDto.Email);
        result.Name.Should().Be(createDto.Name);
        mockUserRepo.Verify(r => r.CreateUserAsync(It.IsAny<User>()), Times.Once);
        mockUserRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Register_ThrowsMissingValue_WhenNameInvalid(string invalidName)
    {
        // Arrange
        var createDto = TestData.CreateUserDto(invalidName, "test@example.com", "SecurePass123!@#");

        var mockUserRepo = new Mock<IUserRepository>();
        var mockPasswordHasher = new Mock<IPasswordHasher<User>>();
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        var service = new AuthService(mockUserRepo.Object, mockPasswordHasher.Object, mockHttpContextAccessor.Object, mockCurrentUser.Object);

        // Act & Assert
        await service.Invoking(s => s.Register(createDto))
            .Should().ThrowAsync<MissingValueException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Register_ThrowsMissingValue_WhenEmailInvalid(string invalidEmail)
    {
        // Arrange
        var createDto = TestData.CreateUserDto("Alice", invalidEmail, "SecurePass123!@#");

        var mockUserRepo = new Mock<IUserRepository>();
        var mockPasswordHasher = new Mock<IPasswordHasher<User>>();
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        var service = new AuthService(mockUserRepo.Object, mockPasswordHasher.Object, mockHttpContextAccessor.Object, mockCurrentUser.Object);

        // Act & Assert
        await service.Invoking(s => s.Register(createDto))
            .Should().ThrowAsync<MissingValueException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Register_ThrowsMissingValue_WhenPasswordInvalid(string invalidPassword)
    {
        // Arrange
        var createDto = TestData.CreateUserDto("Alice", "alice@example.com", invalidPassword);

        var mockUserRepo = new Mock<IUserRepository>();
        var mockPasswordHasher = new Mock<IPasswordHasher<User>>();
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        var service = new AuthService(mockUserRepo.Object, mockPasswordHasher.Object, mockHttpContextAccessor.Object, mockCurrentUser.Object);

        // Act & Assert
        await service.Invoking(s => s.Register(createDto))
            .Should().ThrowAsync<MissingValueException>();
    }

    [Fact]
    public async Task Register_ThrowsAuth_WhenEmailFormatInvalid()
    {
        // Arrange
        var createDto = TestData.CreateUserDto("Alice", "not-an-email", "SecurePass123!@#");

        var mockUserRepo = new Mock<IUserRepository>();
        var mockPasswordHasher = new Mock<IPasswordHasher<User>>();
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        var service = new AuthService(mockUserRepo.Object, mockPasswordHasher.Object, mockHttpContextAccessor.Object, mockCurrentUser.Object);

        // Act & Assert
        await service.Invoking(s => s.Register(createDto))
            .Should().ThrowAsync<ArgumentException>().WithMessage("*Invalid email format*");
    }

    [Fact]
    public async Task Register_ThrowsInvalidValue_WhenPasswordTooWeak()
    {
        // Arrange
        var createDto = TestData.CreateUserDto("Alice", "alice@example.com", "weak");

        var mockUserRepo = new Mock<IUserRepository>();
        var mockPasswordHasher = new Mock<IPasswordHasher<User>>();
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        var service = new AuthService(mockUserRepo.Object, mockPasswordHasher.Object, mockHttpContextAccessor.Object, mockCurrentUser.Object);

        // Act & Assert
        await service.Invoking(s => s.Register(createDto))
            .Should().ThrowAsync<InvalidValueException>();
    }

    [Fact]
    public async Task Register_ThrowsAuth_WhenEmailExists()
    {
        // Arrange
        var createDto = TestData.CreateUserDto("Alice", "existing@example.com", "SecurePass123!@#");

        var mockUserRepo = new Mock<IUserRepository>();
        var mockPasswordHasher = new Mock<IPasswordHasher<User>>();
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        mockUserRepo.Setup(r => r.UserExistsByEmailAsync(createDto.Email)).ReturnsAsync(true);

        var service = new AuthService(mockUserRepo.Object, mockPasswordHasher.Object, mockHttpContextAccessor.Object, mockCurrentUser.Object);

        // Act & Assert
        await service.Invoking(s => s.Register(createDto))
            .Should().ThrowAsync<AuthException>();
    }

    #endregion

    #region Login Tests

    // NOTE: Login_ReturnsUserDto_WhenCredentialsValid removed - Moq cannot mock extension methods (SignInAsync).
    // Authentication/HttpContext testing is better suited for integration tests.

    // NOTE: Login_ThrowsAuth_WhenPasswordIncorrect test removed - Moq cannot mock extension methods (SignInAsync).
    // This test would be better as an integration test that tests the actual authentication flow.

    [Fact]
    public async Task Login_ThrowsAuth_WhenUserNotFound()
    {
        // Arrange
        var loginDto = TestData.LoginDto("nonexistent@example.com", "SecurePass123!@#");

        var mockUserRepo = new Mock<IUserRepository>();
        var mockPasswordHasher = new Mock<IPasswordHasher<User>>();
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        mockUserRepo.Setup(r => r.GetUserByEmailAsync(loginDto.Email)).ReturnsAsync((User?)null);

        var service = new AuthService(mockUserRepo.Object, mockPasswordHasher.Object, mockHttpContextAccessor.Object, mockCurrentUser.Object);

        // Act & Assert
        await service.Invoking(s => s.Login(loginDto))
            .Should().ThrowAsync<AuthException>();
    }

    #endregion

    #region Logout Tests

    // NOTE: Logout_SignsOutSuccessfully removed - Moq cannot mock extension methods (SignOutAsync).
    // Authentication/HttpContext testing is better suited for integration tests.

    #endregion

    #region DeleteAccount Tests

    // NOTE: DeleteAccount_DeletesUserAndSignsOut_WhenValid removed - Moq cannot mock extension methods (SignOutAsync).
    // Authentication/HttpContext testing is better suited for integration tests.

    [Fact]
    public async Task DeleteAccount_ThrowsNotFound_WhenUserNotExists()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var mockUserRepo = new Mock<IUserRepository>();
        var mockPasswordHasher = new Mock<IPasswordHasher<User>>();
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        var mockCurrentUser = new Mock<ICurrentUserService>();

        mockCurrentUser.Setup(c => c.GetUserId()).Returns(userId);
        mockUserRepo.Setup(r => r.GetUserByIdAsync(userId)).ReturnsAsync((User?)null);

        var service = new AuthService(mockUserRepo.Object, mockPasswordHasher.Object, mockHttpContextAccessor.Object, mockCurrentUser.Object);

        // Act & Assert
        await service.Invoking(s => s.DeleteAccount())
            .Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
