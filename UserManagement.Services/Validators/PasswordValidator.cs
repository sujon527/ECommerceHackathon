using System.Text.RegularExpressions;
using UserManagement.Core.DTOs;
using UserManagement.Core.Interfaces;

namespace UserManagement.Services.Validators;

public class PasswordValidator : IValidator<CreateUserDto>
{
    private readonly string[] _weakPasswords = { "password", "12345678", "qwertyuiop", "password123" };

    public Task<(bool IsValid, string? ErrorMessage)> ValidateAsync(CreateUserDto context)
    {
        var password = context.Password;
        var email = context.Email;
        var phoneNumber = context.PhoneNumber;

        if (string.IsNullOrWhiteSpace(password)) return Task.FromResult((false, "Password is required."));
        
        if (password.Length < 10) return Task.FromResult((false, "Password must be at least 10 characters long."));

        bool hasUpper = Regex.IsMatch(password, "[A-Z]");
        bool hasLower = Regex.IsMatch(password, "[a-z]");
        bool hasDigit = Regex.IsMatch(password, "[0-9]");
        bool hasSpecial = Regex.IsMatch(password, "[!@#$%^&*(),.?\":{}|<>]");

        if (!hasUpper || !hasLower || !hasDigit || !hasSpecial)
        {
            return Task.FromResult((false, "Password must include uppercase, lowercase, number, and special character."));
        }

        var emailPrefix = email.Split('@')[0];
        if (password.Contains(emailPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult((false, "Password must not contain the email prefix."));
        }

        if (!string.IsNullOrEmpty(phoneNumber) && phoneNumber.Length >= 6)
        {
            var phoneDigits = Regex.Replace(phoneNumber, @"[^\d]", "");
            if (phoneDigits.Length >= 6)
            {
                var phoneSuffix = phoneDigits.Substring(phoneDigits.Length - 6);
                if (password.Contains(phoneSuffix))
                {
                    return Task.FromResult((false, "Password must not contain the last 6 digits of your mobile number."));
                }
            }
        }

        if (_weakPasswords.Contains(password.ToLower()))
        {
            return Task.FromResult((false, "This password is too common and weak."));
        }

        return Task.FromResult((true, (string?)null));
    }
}
