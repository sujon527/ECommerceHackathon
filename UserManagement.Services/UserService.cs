using UserManagement.Core.DTOs;
using UserManagement.Core.Entities;
using UserManagement.Core.Interfaces;
using BCrypt.Net;
using System.Text.RegularExpressions;

namespace UserManagement.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordValidator _passwordValidator;

    public UserService(IUserRepository userRepository, IPasswordValidator passwordValidator)
    {
        _userRepository = userRepository;
        _passwordValidator = passwordValidator;
    }

    public async Task<UserDto> RegisterUserAsync(CreateUserDto createUserDto)
    {
        // 1. Normalization
        var normalizedEmail = createUserDto.Email.Trim().ToLowerInvariant();
        var normalizedPhone = NormalizePhoneNumber(createUserDto.PhoneNumber);

        // 2. Identity & Uniqueness (Including soft-deleted)
        var existingByEmail = await _userRepository.GetByEmailIncludingDeletedAsync(normalizedEmail);
        if (existingByEmail != null)
        {
            throw new Exception("Registration blocked. Email already exists (may be inactive). Please contact admin.");
        }

        if (!string.IsNullOrEmpty(normalizedPhone))
        {
            var existingByPhone = await _userRepository.GetByPhoneNumberIncludingDeletedAsync(normalizedPhone);
            if (existingByPhone != null)
            {
                throw new Exception("Registration blocked. Phone number already exists (may be inactive). Please contact admin.");
            }
        }

        // 3. Name Validation
        ValidateNames(createUserDto.FirstName, createUserDto.LastName);

        // 4. Age Validation (13+)
        if (createUserDto.DateOfBirth.HasValue)
        {
            var age = CalculateAge(createUserDto.DateOfBirth.Value);
            if (age < 13)
            {
                throw new Exception("Policy violation. Users must be at least 13 years old.");
            }
        }

        // 5. Password Validation
        var (isPassValid, passError) = _passwordValidator.Validate(createUserDto.Password, normalizedEmail, normalizedPhone);
        if (!isPassValid)
        {
            throw new Exception(passError);
        }

        // 6. User Creation & Hashing
        var user = new User
        {
            Username = createUserDto.Username,
            Email = normalizedEmail,
            PhoneNumber = normalizedPhone,
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
        // Simple E.164-ish normalization for Bangladesh as example
        var digits = Regex.Replace(phone, @"[^\d]", "");
        if (digits.StartsWith("01") && digits.Length == 11) return "+88" + digits;
        if (digits.StartsWith("8801") && digits.Length == 13) return "+" + digits;
        return "+" + digits; // Fallback
    }

    private void ValidateNames(string first, string last)
    {
        var nameRegex = new Regex(@"^[a-zA-Z\s\-']+$");
        if (string.IsNullOrWhiteSpace(first) || first.Length < 2 || first.Length > 50 || !nameRegex.IsMatch(first))
            throw new Exception("Invalid First Name. Must be 2-50 characters and contain only letters, spaces, hyphens, or apostrophes.");
        
        if (string.IsNullOrWhiteSpace(last) || last.Length < 2 || last.Length > 50 || !nameRegex.IsMatch(last))
            throw new Exception("Invalid Last Name. Must be 2-50 characters and contain only letters, spaces, hyphens, or apostrophes.");
    }

    private int CalculateAge(DateTime dob)
    {
        var today = DateTime.Today;
        var age = today.Year - dob.Year;
        if (dob.Date > today.AddYears(-age)) age--;
        return age;
    }
}
