using MongoDB.Driver;
using UserManagement.Core.Interfaces;
using UserManagement.Data.Configuration;
using Microsoft.Extensions.Options;

namespace UserManagement.Data.Repositories;

public class MongoRepository<T> : IRepository<T> where T : class
{
    protected readonly IMongoCollection<T> _collection;

    public MongoRepository(IOptions<MongoSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        var database = client.GetDatabase(settings.Value.DatabaseName);
        _collection = database.GetCollection<T>(typeof(T).Name.ToLower() + "s");
    }

    public async Task<T> GetByIdAsync(string id)
    {
        var filter = Builders<T>.Filter.And(
            Builders<T>.Filter.Eq("Id", id),
            Builders<T>.Filter.Ne("IsDeleted", true)
        );
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _collection.Find(Builders<T>.Filter.Ne("IsDeleted", true)).ToListAsync();
    }

    public async Task AddAsync(T entity)
    {
        await _collection.InsertOneAsync(entity);
    }

    public async Task UpdateAsync(T entity)
    {
        var id = typeof(T).GetProperty("Id")?.GetValue(entity)?.ToString();
        var filter = Builders<T>.Filter.And(
            Builders<T>.Filter.Eq("Id", id),
            Builders<T>.Filter.Ne("IsDeleted", true)
        );
        await _collection.ReplaceOneAsync(filter, entity);
    }

    public async Task DeleteAsync(string id)
    {
        var filter = Builders<T>.Filter.Eq("Id", id);
        var update = Builders<T>.Update.Set("IsDeleted", true);
        await _collection.UpdateOneAsync(filter, update);
    }
}
