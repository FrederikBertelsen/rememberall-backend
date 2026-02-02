using FluentAssertions;
using RememberAll.src.Utilities;

namespace RememberAllBackend.Tests.Unit.Validators;

public class EmailValidatorTests
{
    #region Valid Email Tests

    [Fact]
    public void Validate_WithValidEmail_ReturnsTrue()
    {
        // Arrange
        var email = "user@example.com";

        // Act
        var result = EmailValidator.Validate(email);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithValidEmailMultipleDots_ReturnsTrue()
    {
        // Arrange
        var email = "user@mail.example.co.uk";

        // Act
        var result = EmailValidator.Validate(email);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithValidEmailNumericLocal_ReturnsTrue()
    {
        // Arrange
        var email = "123@example.com";

        // Act
        var result = EmailValidator.Validate(email);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithValidEmailSpecialCharsLocal_ReturnsTrue()
    {
        // Arrange
        var email = "user+tag@example.com";

        // Act
        var result = EmailValidator.Validate(email);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithValidEmailSingleCharLocal_ReturnsTrue()
    {
        // Arrange
        var email = "a@example.com";

        // Act
        var result = EmailValidator.Validate(email);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithValidEmailLeadingTrailingWhitespace_ReturnsTrue()
    {
        // Arrange
        var email = "  user@example.com  ";

        // Act
        var result = EmailValidator.Validate(email);

        // Assert
        result.Should().BeTrue();
    }

    #endregion

    #region Invalid Email - Null/Empty/Whitespace Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Validate_WithNullOrEmptyOrWhitespace_ReturnsFalse(string? email)
    {
        // Act
        var result = EmailValidator.Validate(email!);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Invalid Email - @ Symbol Tests

    [Fact]
    public void Validate_WithNoAtSymbol_ReturnsFalse()
    {
        // Arrange
        var email = "userexample.com";

        // Act
        var result = EmailValidator.Validate(email);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithMultipleAtSymbols_ReturnsFalse()
    {
        // Arrange
        var email = "user@@example.com";

        // Act
        var result = EmailValidator.Validate(email);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithMultipleAtSymbolsInDomain_ReturnsFalse()
    {
        // Arrange
        var email = "user@exam@ple.com";

        // Act
        var result = EmailValidator.Validate(email);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithAtSymbolAtStart_ReturnsFalse()
    {
        // Arrange
        var email = "@example.com";

        // Act
        var result = EmailValidator.Validate(email);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithAtSymbolAtEnd_ReturnsFalse()
    {
        // Arrange
        var email = "user@";

        // Act
        var result = EmailValidator.Validate(email);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Invalid Email - Domain Tests

    [Fact]
    public void Validate_WithDomainNoDot_ReturnsFalse()
    {
        // Arrange
        var email = "user@example";

        // Act
        var result = EmailValidator.Validate(email);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithDomainEndingInDot_ReturnsFalse()
    {
        // Arrange
        var email = "user@example.";

        // Act
        var result = EmailValidator.Validate(email);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithDomainOnlyDot_ReturnsFalse()
    {
        // Arrange
        var email = "user@.";

        // Act
        var result = EmailValidator.Validate(email);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Invalid Email - Space Tests

    [Fact]
    public void Validate_WithSpaceInLocalPart_ReturnsFalse()
    {
        // Arrange
        var email = "user name@example.com";

        // Act
        var result = EmailValidator.Validate(email);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithSpaceInDomain_ReturnsFalse()
    {
        // Arrange
        var email = "user@exam ple.com";

        // Act
        var result = EmailValidator.Validate(email);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithSpaceBeforeAt_ReturnsFalse()
    {
        // Arrange
        var email = "user @example.com";

        // Act
        var result = EmailValidator.Validate(email);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithSpaceAfterAt_ReturnsFalse()
    {
        // Arrange
        var email = "user@ example.com";

        // Act
        var result = EmailValidator.Validate(email);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Validate_WithLongLocalPart_ReturnsTrue()
    {
        // Arrange
        var email = "verylongemailaddresslocalpartwithnumbers12345@example.com";

        // Act
        var result = EmailValidator.Validate(email);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithMultipleDotsInDomain_ReturnsTrue()
    {
        // Arrange
        var email = "user@mail.example.co.uk";

        // Act
        var result = EmailValidator.Validate(email);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithSingleCharDomain_ReturnsTrue()
    {
        // Arrange
        var email = "user@a.co";

        // Act
        var result = EmailValidator.Validate(email);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithNumericDomain_ReturnsTrue()
    {
        // Arrange
        var email = "user@123.456.com";

        // Act
        var result = EmailValidator.Validate(email);

        // Assert
        result.Should().BeTrue();
    }

    #endregion
}
