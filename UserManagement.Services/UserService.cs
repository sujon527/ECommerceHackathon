using UserManagement.Core.DTOs;
using UserManagement.Core.Entities;
using UserManagement.Core.Interfaces;
using BCrypt.Net;
using System.Text.RegularExpressions;
using UserManagement.Services.Validators;

namespace UserManagement.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IValidator<IUserValidationFields> _nameValidator;
    private readonly IValidator<IUserValidationFields> _ageValidator;
    private readonly IValidator<IUserValidationFields> _passwordValidator;
    private readonly IValidator<IUserValidationFields> _uniquenessValidator;

    public UserService(
        IUserRepository userRepository,
        NameValidator nameValidator,
        AgeValidator ageValidator,
        PasswordValidator passwordValidator,
        UniquenessValidator uniquenessValidator)
    {
        _userRepository = userRepository;
        _nameValidator = nameValidator;
        _ageValidator = ageValidator;
        _passwordValidator = passwordValidator;
        _uniquenessValidator = uniquenessValidator;
    }

    public async Task<UserDto> RegisterUserAsync(CreateUserDto createUserDto)
    {
        // 1. Normalization
        createUserDto.Email = createUserDto.Email.Trim().ToLowerInvariant();
        createUserDto.PhoneNumber = NormalizePhoneNumber(createUserDto.PhoneNumber);

        // 2. Service-Led Validation Orchestration
        await ValidateOrThrowAsync(_nameValidator, createUserDto);
        await ValidateOrThrowAsync(_ageValidator, createUserDto);
        await ValidateOrThrowAsync(_passwordValidator, createUserDto);
        await ValidateOrThrowAsync(_uniquenessValidator, createUserDto);

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

        return MapToDto(user);
    }

    public async Task<UserDto> UpdateUserAsync(string id, UpdateUserDto updateUserDto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) throw new Exception("User not found.");

        updateUserDto.UserId = id;
        updateUserDto.PhoneNumber = NormalizePhoneNumber(updateUserDto.PhoneNumber);

        // 2. Service-Led Validation Orchestration (Selecting only relevant validators)
        await ValidateOrThrowAsync(_nameValidator, updateUserDto);
        await ValidateOrThrowAsync(_ageValidator, updateUserDto);
        await ValidateOrThrowAsync(_uniquenessValidator, updateUserDto);
        // PASSWORD VALIDATOR IS EXPLICITLY OMITTED FOR UPDATE

        user.FirstName = updateUserDto.FirstName;
        user.LastName = updateUserDto.LastName;
        user.PhoneNumber = updateUserDto.PhoneNumber;
        user.DateOfBirth = updateUserDto.DateOfBirth;
        user.DisplayName = updateUserDto.DisplayName ?? $"{updateUserDto.FirstName} {updateUserDto.LastName}";

        await _userRepository.UpdateAsync(user);

        return MapToDto(user);
    }

    public async Task<UserDto?> GetByIdAsync(string id)
    {
        var user = await _userRepository.GetByIdIncludingDeletedAsync(id);
        return user != null ? MapToDto(user) : null;
    }

    public async Task<List<UserDto>> GetActiveUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return users.Select(MapToDto).ToList();
    }

    public async Task<List<UserDto>> GetInactiveUsersAsync()
    {
        var users = await _userRepository.GetInactiveAsync();
        return users.Select(MapToDto).ToList();
    }

    public async Task DeleteUserAsync(string id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) throw new Exception("User not found.");
        await _userRepository.DeleteAsync(id);
    }

    public async Task ActivateUserAsync(string id)
    {
        var user = await _userRepository.GetByIdIncludingDeletedAsync(id);
        if (user == null) throw new Exception("User not found.");
        await _userRepository.ActivateAsync(id);
    }

    private async Task ValidateOrThrowAsync(IValidator<IUserValidationFields> validator, IUserValidationFields fields)
    {
        var (isValid, errorMessage) = await validator.ValidateAsync(fields);
        if (!isValid) throw new Exception(errorMessage);
    }

    private UserDto MapToDto(User user)
    {
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
