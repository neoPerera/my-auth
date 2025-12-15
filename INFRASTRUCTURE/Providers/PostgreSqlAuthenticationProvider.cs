using CORE.Entities;
using CORE.Interfaces;
using INFRASTRUCTURE.Persistence;
using Microsoft.EntityFrameworkCore;

namespace INFRASTRUCTURE.Providers;

public class PostgreSqlAuthenticationProvider : IAuthenticationProvider
{
    private readonly AppDbContext _context;

    public PostgreSqlAuthenticationProvider(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User> CreateUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> UserExistsAsync(string username)
    {
        return await _context.Users
            .AnyAsync(u => u.Username == username);
    }
}

