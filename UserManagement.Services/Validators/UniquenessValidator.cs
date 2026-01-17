using UserManagement.Core.DTOs;
using UserManagement.Core.Interfaces;

namespace UserManagement.Services.Validators;

public class UniquenessValidator : IValidator<CreateUserDto>
{
    private readonly IUserRepository _userRepository;

    public UniquenessValidator(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<(bool IsValid, string? ErrorMessage)> ValidateAsync(CreateUserDto context)
    {
        var normalizedEmail = context.Email.Trim().ToLowerInvariant();
        var normalizedPhone = context.PhoneNumber; // Assuming it's already normalized or handled by UserService for now

        var existingByEmail = await _userRepository.GetByEmailIncludingDeletedAsync(normalizedEmail);
        if (existingByEmail != null)
        {
            return (false, "Registration blocked. Email already exists (may be inactive). Please contact admin.");
        }

        if (!string.IsNullOrEmpty(normalizedPhone))
        {
            var existingByPhone = await _userRepository.GetByPhoneNumberIncludingDeletedAsync(normalizedPhone);
            if (existingByPhone != null)
            {
                return (false, "Registration blocked. Phone number already exists (may be inactive). Please contact admin.");
            }
        }

        return (true, null);
    }
}
