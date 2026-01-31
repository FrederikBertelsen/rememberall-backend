namespace RememberAll.src.Utilities;

public class PasswordValidationResult
{
    public required bool IsValid { get; init; }
    public required string ValidationErrors { get; init; }
}

public static class PasswordValidator
{
    // https://owasp.org/www-community/password-special-characters
    private const string SpecialCharacters = " !\"#$%&'()*+,-./:;<=>?@[\\]^_{|}~";
    private const int MinimumPasswordLength = 8;
    private const int MinimumUppercaseLetters = 1;
    private const int MinimumLowercaseLetters = 1;
    private const int MinimumDigits = 1;
    private const int MinimumSpecialCharacters = 0;

    public static PasswordValidationResult Validate(string password)
    {
        int uppercaseLetterCount = 0, lowercaseLetterCount = 0, digitCount = 0, specialCharacterCount = 0;

        foreach (char c in password)
        {
            if (char.IsUpper(c)) uppercaseLetterCount++;
            else if (char.IsLower(c)) lowercaseLetterCount++;
            else if (char.IsDigit(c)) digitCount++;
            else if (SpecialCharacters.Contains(c)) specialCharacterCount++;
        }

        bool isValid = false;

        List<string> errors = [];

        if (password.Length < MinimumPasswordLength)
            errors.Add($"Password must be at least {MinimumPasswordLength} characters long.");
        if (uppercaseLetterCount < MinimumUppercaseLetters)
            errors.Add($"Password must contain at least {MinimumUppercaseLetters} uppercase letter(s).");
        if (lowercaseLetterCount < MinimumLowercaseLetters)
            errors.Add($"Password must contain at least {MinimumLowercaseLetters} lowercase letter(s).");
        if (digitCount < MinimumDigits)
            errors.Add($"Password must contain at least {MinimumDigits} digit(s).");
        if (specialCharacterCount < MinimumSpecialCharacters)
            errors.Add($"Password must contain at least {MinimumSpecialCharacters} special character(s).");

        if (errors.Count == 0)
            isValid = true;

        return new PasswordValidationResult
        {
            IsValid = isValid,
            ValidationErrors = string.Join("\n", errors)
        };
    }

    public static string GetRequirementsMessage()
    {
        return $"Password requirements:\n" +
               $"- At least {MinimumPasswordLength} characters long\n" +
               $"- At least {MinimumUppercaseLetters} uppercase letter(s)\n" +
               $"- At least {MinimumLowercaseLetters} lowercase letter(s)\n" +
               $"- At least {MinimumDigits} digit(s)\n" +
               $"- At least {MinimumSpecialCharacters} special character(s)";
    }
}