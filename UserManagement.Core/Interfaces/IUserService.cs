using UserManagement.Core.DTOs;

namespace UserManagement.Core.Interfaces;

public interface IUserService
{
    Task<UserDto> RegisterUserAsync(CreateUserDto createUserDto);
    Task<UserDto> UpdateUserAsync(string id, UpdateUserDto updateUserDto);
    Task<UserDto?> GetByIdAsync(string id);
    Task<List<UserDto>> GetActiveUsersAsync();
    Task<List<UserDto>> GetInactiveUsersAsync();
    Task DeleteUserAsync(string id);
    Task ActivateUserAsync(string id);
}
