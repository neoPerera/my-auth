using CORE.Interfaces;
using INFRASTRUCTURE.Persistence;
using Microsoft.EntityFrameworkCore;

namespace INFRASTRUCTURE.Providers;

public class DatabaseAuthorizationProvider : IAuthorizationProvider
{
    private readonly AppDbContext _context;

    public DatabaseAuthorizationProvider(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<string>> GetUserRolesAsync(string username)
    {
        // Since we're using permissions directly, not roles, return empty list
        // Or you can implement roles if you have a roles table
        return await Task.FromResult(new List<string>());
    }

    public async Task<List<string>> GetRolePermissionsAsync(string role)
    {
        // Since we're using user permissions directly, not role-based permissions
        // Return empty list or implement if you have role-permission mapping
        return await Task.FromResult(new List<string>());
    }

    public async Task<bool> UserHasPermissionAsync(string username, string permission)
    {
        // Check if user has the permission directly
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
        {
            return false;
        }

        // Check for wildcard permission "*"
        var hasWildcard = await _context.UserPermissions
            .Include(up => up.Permission)
            .Include(up => up.User)
            .Where(up => up.UserId == user.Id && up.Permission.PermissionName == "*")
            .AnyAsync();

        if (hasWildcard)
        {
            return true;
        }

        // Check for specific permission
        var hasPermission = await _context.UserPermissions
            .Include(up => up.Permission)
            .Include(up => up.User)
            .Where(up => up.UserId == user.Id && up.Permission.PermissionName == permission)
            .AnyAsync();

        return hasPermission;
    }

    public async Task<bool> UserHasAnyRoleAsync(string username, params string[] roles)
    {
        // Since we're using permissions directly, not roles
        // Return false or implement if you have roles table
        return await Task.FromResult(false);
    }

    // Additional helper method to get all user permissions
    public async Task<List<string>> GetUserPermissionsAsync(string username)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
        {
            return new List<string>();
        }

        var permissions = await _context.UserPermissions
            .Include(up => up.Permission)
            .Where(up => up.UserId == user.Id)
            .Select(up => up.Permission.PermissionName)
            .ToListAsync();

        return permissions;
    }
}

