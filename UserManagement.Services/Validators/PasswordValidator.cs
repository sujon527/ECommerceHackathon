using System.Text.RegularExpressions;
using UserManagement.Core.Interfaces;

namespace UserManagement.Services.Validators;

public class PasswordValidator : IPasswordValidator
{
    private readonly string[] _weakPasswords = { "password", "12345678", "qwertyuiop", "password123" }; // Simplified list

    public (bool IsValid, string? ErrorMessage) Validate(string password, string email, string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(password)) return (false, "Password is required.");
        
        // 1. Min length 10
        if (password.Length < 10) return (false, "Password must be at least 10 characters long.");

        // 2. Complexity
        bool hasUpper = Regex.IsMatch(password, "[A-Z]");
        bool hasLower = Regex.IsMatch(password, "[a-z]");
        bool hasDigit = Regex.IsMatch(password, "[0-9]");
        bool hasSpecial = Regex.IsMatch(password, "[!@#$%^&*(),.?\":{}|<>]");

        if (!hasUpper || !hasLower || !hasDigit || !hasSpecial)
        {
            return (false, "Password must include uppercase, lowercase, number, and special character.");
        }

        // 3. Email prefix check
        var emailPrefix = email.Split('@')[0];
        if (password.Contains(emailPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Password must not contain the email prefix.");
        }

        // 4. Mobile substring check (last 6 digits)
        if (!string.IsNullOrEmpty(phoneNumber) && phoneNumber.Length >= 6)
        {
            var phoneSuffix = phoneNumber.Substring(phoneNumber.Length - 6);
            if (password.Contains(phoneSuffix))
            {
                return (false, "Password must not contain the last 6 digits of your mobile number.");
            }
        }

        // 5. Weak password check
        if (_weakPasswords.Contains(password.ToLower()))
        {
            return (false, "This password is too common and weak.");
        }

        return (true, null);
    }
}
