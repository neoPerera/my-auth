namespace CORE.Entities;

public class Permission
{
    public int Id { get; set; }
    public string PermissionName { get; set; } = string.Empty;
    public string? PermissionDescription { get; set; }
    
    // Navigation property
    public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}

