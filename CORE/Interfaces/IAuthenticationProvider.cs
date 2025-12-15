using CORE.Entities;

namespace CORE.Interfaces;

public interface IAuthenticationProvider
{
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByIdAsync(int id);
    Task<User> CreateUserAsync(User user);
    Task<User> UpdateUserAsync(User user);
    Task<bool> UserExistsAsync(string username);
}

