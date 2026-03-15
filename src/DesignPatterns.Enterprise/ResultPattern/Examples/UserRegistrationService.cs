namespace DesignPatterns.Enterprise.ResultPattern.Examples;

/// <summary>
/// Demonstrates the Result pattern in a realistic user-registration flow.
/// Each step can fail independently, and errors are propagated without exceptions.
/// </summary>
public sealed class UserRegistrationService
{
    private readonly HashSet<string> _existingEmails = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<UserAccount> _accounts = new();

    public Result<UserAccount> Register(string email, string password, string fullName)
    {
        return ValidateEmail(email)
            .Bind(_ => ValidatePassword(password))
            .Bind(_ => CheckDuplicate(email))
            .Bind(_ => CreateAccount(email, password, fullName));
    }

    private static Result<string> ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Error.Validation("Email.Empty", "Email is required.");
        if (!email.Contains('@'))
            return Error.Validation("Email.Invalid", "Email must contain '@'.");
        return email;
    }

    private static Result<string> ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return Error.Validation("Password.Empty", "Password is required.");
        if (password.Length < 8)
            return Error.Validation("Password.TooShort", "Password must be at least 8 characters.");
        if (!password.Any(char.IsDigit))
            return Error.Validation("Password.NoDigit", "Password must contain at least one digit.");
        return password;
    }

    private Result<string> CheckDuplicate(string email)
    {
        if (_existingEmails.Contains(email))
            return Error.Conflict("Email.Duplicate", $"The email '{email}' is already registered.");
        return email;
    }

    private Result<UserAccount> CreateAccount(string email, string password, string fullName)
    {
        var account = new UserAccount(Guid.NewGuid(), email, fullName, DateTime.UtcNow);
        _accounts.Add(account);
        _existingEmails.Add(email);
        return account;
    }
}

public sealed record UserAccount(Guid Id, string Email, string FullName, DateTime CreatedAt);
