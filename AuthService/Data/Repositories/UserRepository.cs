using System.Data;
using AuthService.Data.Entities;
using AuthService.Data.Repositories.Interfaces;
using Dapper;

namespace AuthService.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbConnection _db;

    public UserRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        const string sql = "SELECT * FROM users WHERE email = @Email";
        return await _db.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        const string sql = "SELECT * FROM users WHERE id = @Id";
        return await _db.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
    }

    public async Task<int> CreateAsync(User user)
    {
        const string sql = @"
            INSERT INTO users (username, email, password_hash, created_at)
            VALUES (@Username, @Email, @PasswordHash, @CreatedAt)
            RETURNING id";

        return await _db.ExecuteScalarAsync<int>(sql, user);
    }
}