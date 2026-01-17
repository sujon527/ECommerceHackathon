using MongoDB.Driver;
using UserManagement.Core.Entities;
using UserManagement.Core.Interfaces;
using UserManagement.Data.Configuration;
using Microsoft.Extensions.Options;

namespace UserManagement.Data.Repositories;

public class UserRepository : MongoRepository<User>, IUserRepository
{
    public UserRepository(IOptions<MongoSettings> settings) : base(settings)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _collection.Find(u => u.Email == email && !u.IsDeleted).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByPhoneNumberAsync(string phoneNumber)
    {
        return await _collection.Find(u => u.PhoneNumber == phoneNumber && !u.IsDeleted).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByEmailIncludingDeletedAsync(string email)
    {
        return await _collection.Find(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByPhoneNumberIncludingDeletedAsync(string phoneNumber)
    {
        return await _collection.Find(u => u.PhoneNumber == phoneNumber).FirstOrDefaultAsync();
    }

    public async Task<List<User>> GetInactiveAsync()
    {
        return await _collection.Find(u => u.IsDeleted).ToListAsync();
    }

    public async Task<User?> GetByIdIncludingDeletedAsync(string id)
    {
        return await _collection.Find(u => u.Id == id).FirstOrDefaultAsync();
    }

    public async Task ActivateAsync(string id)
    {
        var filter = Builders<User>.Filter.Eq("Id", id);
        var update = Builders<User>.Update.Set("IsDeleted", false);
        await _collection.UpdateOneAsync(filter, update);
    }
}
