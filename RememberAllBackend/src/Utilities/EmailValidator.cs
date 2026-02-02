namespace RememberAll.src.Utilities;

public static class EmailValidator
{
    public static bool Validate(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        email = email.Trim();

        // Must contain exactly one @ symbol
        if (!email.Contains('@') || email.IndexOf('@') != email.LastIndexOf('@'))
            return false;

        int atIndex = email.IndexOf('@');

        // Must have local part before @
        if (atIndex == 0)
            return false;

        // Must have domain part after @
        if (atIndex == email.Length - 1)
            return false;

        string domain = email.Substring(atIndex + 1);

        // Domain must contain at least one dot
        if (!domain.Contains('.'))
            return false;

        // Domain cannot end with a dot
        if (domain.EndsWith('.'))
            return false;

        // Cannot contain spaces
        if (email.Contains(' '))
            return false;

        return true;
    }
}
