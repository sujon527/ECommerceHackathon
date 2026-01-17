using UserManagement.Core.DTOs;
using UserManagement.Core.Entities;
using UserManagement.Core.Interfaces;
using BCrypt.Net;
using System.Text.RegularExpressions;

namespace UserManagement.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IEnumerable<IValidator<CreateUserDto>> _validators;

    public UserService(IUserRepository userRepository, IEnumerable<IValidator<CreateUserDto>> validators)
    {
        _userRepository = userRepository;
        _validators = validators;
    }

    public async Task<UserDto> RegisterUserAsync(CreateUserDto createUserDto)
    {
        // 1. Normalization
        createUserDto.Email = createUserDto.Email.Trim().ToLowerInvariant();
        createUserDto.PhoneNumber = NormalizePhoneNumber(createUserDto.PhoneNumber);

        // 2. Modular Validation
        foreach (var validator in _validators)
        {
            var (isValid, errorMessage) = await validator.ValidateAsync(createUserDto);
            if (!isValid)
            {
                throw new Exception(errorMessage);
            }
        }

        // 3. User Creation & Hashing
        var user = new User
        {
            Username = createUserDto.Username,
            Email = createUserDto.Email,
            PhoneNumber = createUserDto.PhoneNumber,
            FirstName = createUserDto.FirstName,
            LastName = createUserDto.LastName,
            DateOfBirth = createUserDto.DateOfBirth,
            DisplayName = createUserDto.DisplayName ?? $"{createUserDto.FirstName} {createUserDto.LastName}",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password),
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _userRepository.AddAsync(user);

        return new UserDto
        {
            Id = user.Id!,
            Username = user.Username,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            FullName = $"{user.FirstName} {user.LastName}",
            DisplayName = user.DisplayName
        };
    }

    private string NormalizePhoneNumber(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return string.Empty;
        var digits = Regex.Replace(phone, @"[^\d]", "");
        if (digits.StartsWith("01") && digits.Length == 11) return "+88" + digits;
        if (digits.StartsWith("8801") && digits.Length == 13) return "+" + digits;
        return "+" + digits;
    }
}
