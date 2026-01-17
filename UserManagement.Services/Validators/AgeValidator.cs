using UserManagement.Core.DTOs;
using UserManagement.Core.Interfaces;

namespace UserManagement.Services.Validators;

public class AgeValidator : IValidator<CreateUserDto>
{
    public Task<(bool IsValid, string? ErrorMessage)> ValidateAsync(CreateUserDto context)
    {
        if (context.DateOfBirth.HasValue)
        {
            var age = CalculateAge(context.DateOfBirth.Value);
            if (age < 13)
            {
                return Task.FromResult((false, "Policy violation. Users must be at least 13 years old."));
            }
        }
        return Task.FromResult((true, (string?)null));
    }

    private int CalculateAge(DateTime dob)
    {
        var today = DateTime.Today;
        var age = today.Year - dob.Year;
        if (dob.Date > today.AddYears(-age)) age--;
        return age;
    }
}
