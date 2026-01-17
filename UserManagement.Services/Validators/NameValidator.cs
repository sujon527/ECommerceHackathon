using System.Text.RegularExpressions;
using UserManagement.Core.DTOs;
using UserManagement.Core.Interfaces;

namespace UserManagement.Services.Validators;

public class NameValidator : IValidator<CreateUserDto>
{
    private static readonly Regex NameRegex = new Regex(@"^[a-zA-Z\s\-']+$", RegexOptions.Compiled);

    public Task<(bool IsValid, string? ErrorMessage)> ValidateAsync(CreateUserDto context)
    {
        if (string.IsNullOrWhiteSpace(context.FirstName) || context.FirstName.Length < 2 || context.FirstName.Length > 50 || !NameRegex.IsMatch(context.FirstName))
        {
            return Task.FromResult((false, "Invalid First Name. Must be 2-50 characters and contain only letters, spaces, hyphens, or apostrophes."));
        }

        if (string.IsNullOrWhiteSpace(context.LastName) || context.LastName.Length < 2 || context.LastName.Length > 50 || !NameRegex.IsMatch(context.LastName))
        {
            return Task.FromResult((false, "Invalid Last Name. Must be 2-50 characters and contain only letters, spaces, hyphens, or apostrophes."));
        }

        return Task.FromResult((true, (string?)null));
    }
}
