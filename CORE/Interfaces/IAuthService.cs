using System.Security.Claims;
using CORE.Entities;

namespace CORE.Interfaces;

public interface IAuthService
{
    Task<AuthResult> AuthenticateAsync(string username, string password);
    Task<AuthResult> AuthenticateMobileAsync(string username, string password);
    Task<ValidationResult> ValidateTokenAsync(string token);
    Task<ChangePasswordResult> ChangePasswordAsync(string username, string currentPassword, string newPassword);
    Task<SignUpResult> SignUpAsync(string username, string password);
}

public class AuthResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Token { get; set; }
    public User? User { get; set; }
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }
    public User? User { get; set; }
    public IEnumerable<Claim>? Claims { get; set; }
}

public class ChangePasswordResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class SignUpResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; }
    public User? User { get; set; }
}

