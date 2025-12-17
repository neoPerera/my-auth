namespace CORE.Entities;

public class UserRole
{
    public string Username { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}

