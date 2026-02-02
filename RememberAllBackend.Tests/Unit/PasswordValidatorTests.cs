using FluentAssertions;
using RememberAll.src.Utilities;

namespace RememberAllBackend.Tests.Unit;

public class PasswordValidatorTests
{
    [Fact]
    public void Validate_ReturnsValid_WhenPasswordMeetsAllRequirements()
    {
        // Arrange & Act
        var result = PasswordValidator.Validate("ValidPass123!");

        // Assert
        result.IsValid.Should().BeTrue();
        result.ValidationErrors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Pass123")] // Too short
    [InlineData("")] // Empty
    public void Validate_ReturnsInvalid_WhenPasswordTooShort(string password)
    {
        // Act
        var result = PasswordValidator.Validate(password);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ValidationErrors.Should().Contain("Password must be at least 8 characters long.");
    }

    [Fact]
    public void Validate_ReturnsInvalid_WhenMissingUppercase()
    {
        // Act
        var result = PasswordValidator.Validate("validpass123!");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ValidationErrors.Should().Contain("Password must contain at least 1 uppercase letter(s).");
    }

    [Fact]
    public void Validate_ReturnsInvalid_WhenMissingLowercase()
    {
        // Act
        var result = PasswordValidator.Validate("VALIDPASS123!");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ValidationErrors.Should().Contain("Password must contain at least 1 lowercase letter(s).");
    }

    [Fact]
    public void Validate_ReturnsInvalid_WhenMissingDigit()
    {
        // Act
        var result = PasswordValidator.Validate("ValidPassword!");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ValidationErrors.Should().Contain("Password must contain at least 1 digit(s).");
    }

    [Fact]
    public void Validate_ReturnsInvalid_WithMultipleViolations()
    {
        // Arrange & Act
        var result = PasswordValidator.Validate("test");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ValidationErrors.Split('\n').Should().HaveCountGreaterThan(1);
    }

    [Theory]
    [InlineData("Pass!123word")]
    [InlineData("Pass@123word")]
    [InlineData("Pass 123word")] // Space is valid
    public void Validate_ReturnsValid_WithSpecialCharacters(string password)
    {
        // Act
        var result = PasswordValidator.Validate(password);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ValidationErrors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ReturnsValid_WhenSpecialCharactersNotRequired()
    {
        // Act - Password without special characters should be valid since MinimumSpecialCharacters = 0
        var result = PasswordValidator.Validate("ValidPass123");

        // Assert
        result.IsValid.Should().BeTrue();
        result.ValidationErrors.Should().NotContain("special character");
    }

    [Fact]
    public void GetRequirementsMessage_ReturnsMessage_WithAllRequirements()
    {
        // Act
        var message = PasswordValidator.GetRequirementsMessage();

        // Assert
        message.Should().NotBeEmpty();
        message.Should().Contain("8");
        message.Should().Contain("uppercase");
        message.Should().Contain("lowercase");
        message.Should().Contain("digit");
    }
}
