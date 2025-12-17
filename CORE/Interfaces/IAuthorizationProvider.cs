using CORE.Entities;

namespace CORE.Interfaces;

public interface IAuthorizationProvider
{
    Task<List<string>> GetUserRolesAsync(string username);
    Task<List<string>> GetRolePermissionsAsync(string role);
    Task<List<string>> GetUserPermissionsAsync(string username);
    Task<bool> UserHasPermissionAsync(string username, string permission);
    Task<bool> UserHasAnyRoleAsync(string username, params string[] roles);
}

