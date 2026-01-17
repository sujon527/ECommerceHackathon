using UserManagement.Core.DTOs;

namespace UserManagement.Core.Interfaces;

public interface IUserService
{
    Task<UserDto> RegisterUserAsync(CreateUserDto createUserDto);
}
