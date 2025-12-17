namespace APPLICATION.DTOs;

public class AuthorizationRequest
{
    public string Token { get; set; } = string.Empty;
    public string Service { get; set; } = string.Empty;
    public string Controller { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Method { get; set; }
}

