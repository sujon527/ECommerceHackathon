using UserManagement.Core.Entities;

namespace UserManagement.Core.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByPhoneNumberAsync(string phoneNumber);
    Task<User?> GetByEmailIncludingDeletedAsync(string email);
    Task<User?> GetByPhoneNumberIncludingDeletedAsync(string phoneNumber);
}
