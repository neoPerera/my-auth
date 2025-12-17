using System.Security.Claims;

namespace APPLICATION.DTOs;

public class AuthorizationResponse
{
    public bool IsAuthorized { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string>? Roles { get; set; }
    public Dictionary<string, string>? Claims { get; set; }
}

