using CORE.Interfaces;

namespace INFRASTRUCTURE.Providers;

public class MockAuthorizationProvider : IAuthorizationProvider
{
    // User roles: [username, roles[]]
    private readonly Dictionary<string, List<string>> _userRoles = new()
    {
        { "admin", new List<string> { "Admin" } },
        { "test2", new List<string> { "user", "manager" } }
    };

    // Role permissions: [role, permissions[]]
    private readonly Dictionary<string, List<string>> _rolePermissions = new()
    {
        { "Admin", new List<string> { "*" } }, // All permissions
        { "user", new List<string> { "Home.GetActiveForms" } },
        { "manager", new List<string> { "Home.Create", "Home.Update" } }
    };

    public Task<List<string>> GetUserRolesAsync(string username)
    {
        _userRoles.TryGetValue(username.ToLowerInvariant(), out var roles);
        return Task.FromResult(roles ?? new List<string>());
    }

    public Task<List<string>> GetRolePermissionsAsync(string role)
    {
        _rolePermissions.TryGetValue(role, out var permissions);
        return Task.FromResult(permissions ?? new List<string>());
    }

    public async Task<List<string>> GetUserPermissionsAsync(string username)
    {
        var roles = await GetUserRolesAsync(username);
        var allPermissions = new List<string>();
        
        foreach (var role in roles)
        {
            var rolePermissions = await GetRolePermissionsAsync(role);
            allPermissions.AddRange(rolePermissions);
        }
        
        return allPermissions.Distinct().ToList();
    }

    public async Task<bool> UserHasPermissionAsync(string username, string permission)
    {
        var roles = await GetUserRolesAsync(username);
        
        foreach (var role in roles)
        {
            var permissions = await GetRolePermissionsAsync(role);
            
            // Check if role has all permissions (Admin with "*")
            if (permissions.Contains("*"))
            {
                return true;
            }
            
            // Check if role has the specific permission
            if (permissions.Contains(permission))
            {
                return true;
            }
        }
        
        return false;
    }

    public async Task<bool> UserHasAnyRoleAsync(string username, params string[] roles)
    {
        var userRoles = await GetUserRolesAsync(username);
        return roles.Any(role => userRoles.Contains(role, StringComparer.OrdinalIgnoreCase));
    }
}

