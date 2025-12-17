using BCrypt.Net;
using CORE.Entities;
using CORE.Interfaces;

namespace INFRASTRUCTURE.Providers;

public class MockAuthenticationProvider : IAuthenticationProvider
{
    // In-memory user storage: [username, password_hash]
    private readonly Dictionary<string, User> _users = new();
    
    // User roles: [username, roles[]]
    private readonly Dictionary<string, List<string>> _userRoles = new();

    public MockAuthenticationProvider()
    {
        InitializeMockData();
    }

    private void InitializeMockData()
    {
        // Create admin user - password: "admin123"
        var adminUser = new User
        {
            Id = 1,
            Username = "admin",
            Password = BCrypt.Net.BCrypt.HashPassword("admin123")
        };
        _users["admin"] = adminUser;
        _userRoles["admin"] = new List<string> { "Admin" };

        // Create test2 user - password: "test123"
        var test2User = new User
        {
            Id = 2,
            Username = "test2",
            Password = BCrypt.Net.BCrypt.HashPassword("test123")
        };
        _users["test2"] = test2User;
        _userRoles["test2"] = new List<string> { "user", "manager" };
    }

    public Task<User?> GetUserByUsernameAsync(string username)
    {
        _users.TryGetValue(username.ToLowerInvariant(), out var user);
        return Task.FromResult(user);
    }

    public Task<User?> GetUserByIdAsync(int id)
    {
        var user = _users.Values.FirstOrDefault(u => u.Id == id);
        return Task.FromResult(user);
    }

    public Task<User> CreateUserAsync(User user)
    {
        var username = user.Username.ToLowerInvariant();
        if (_users.ContainsKey(username))
        {
            throw new InvalidOperationException($"User {username} already exists");
        }

        user.Id = _users.Count + 1;
        _users[username] = user;
        return Task.FromResult(user);
    }

    public Task<User> UpdateUserAsync(User user)
    {
        var username = user.Username.ToLowerInvariant();
        if (!_users.ContainsKey(username))
        {
            throw new InvalidOperationException($"User {username} not found");
        }

        _users[username] = user;
        return Task.FromResult(user);
    }

    public Task<bool> UserExistsAsync(string username)
    {
        return Task.FromResult(_users.ContainsKey(username.ToLowerInvariant()));
    }

    public List<string> GetUserRoles(string username)
    {
        _userRoles.TryGetValue(username.ToLowerInvariant(), out var roles);
        return roles ?? new List<string>();
    }
}

