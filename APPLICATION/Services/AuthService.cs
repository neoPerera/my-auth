using System.Security.Claims;
using CORE.Interfaces;
using CORE.Entities;

namespace APPLICATION.Services;

public class AuthService : IAuthService
{
    private readonly IAuthenticationProvider _authProvider;
    private readonly IAuthorizationProvider? _authorizationProvider;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IInputSanitizer _inputSanitizer;

    public AuthService(
        IAuthenticationProvider authProvider,
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        IInputSanitizer inputSanitizer,
        IAuthorizationProvider? authorizationProvider = null)
    {
        _authProvider = authProvider;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _inputSanitizer = inputSanitizer;
        _authorizationProvider = authorizationProvider;
    }

    public async Task<AuthResult> AuthenticateAsync(string username, string password)
    {
        // Sanitize inputs
        username = _inputSanitizer.SanitizeUsername(username);
        password = _inputSanitizer.SanitizePassword(password);

        // Validate after sanitization
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return new AuthResult
            {
                Success = false,
                Message = "Invalid username or password"
            };
        }

        // Check for malicious content
        if (_inputSanitizer.ContainsMaliciousContent(username) || _inputSanitizer.ContainsMaliciousContent(password))
        {
            return new AuthResult
            {
                Success = false,
                Message = "Invalid username or password"
            };
        }

        var user = await _authProvider.GetUserByUsernameAsync(username);
        
        if (user == null)
        {
            return new AuthResult
            {
                Success = false,
                Message = "Invalid username or password"
            };
        }

        if (!_passwordHasher.VerifyPassword(password, user.Password))
        {
            return new AuthResult
            {
                Success = false,
                Message = "Invalid username or password"
            };
        }

        // Get user roles and permissions if authorization provider is available
        List<string>? roles = null;
        List<string>? permissions = null;
        if (_authorizationProvider != null)
        {
            roles = await _authorizationProvider.GetUserRolesAsync(user.Username);
            
            // Get permissions directly from user (for database provider) or from roles (for mock provider)
            permissions = await _authorizationProvider.GetUserPermissionsAsync(user.Username);
        }

        var token = _tokenService.GenerateToken(user, roles, permissions);

        return new AuthResult
        {
            Success = true,
            Token = token,
            User = user,
            Message = "Authentication successful"
        };
    }

    public async Task<AuthResult> AuthenticateMobileAsync(string username, string password)
    {
        // Mobile authentication can have different logic if needed
        return await AuthenticateAsync(username, password);
    }

    public async Task<ValidationResult> ValidateTokenAsync(string token)
    {
        // Sanitize token
        token = _inputSanitizer.SanitizeToken(token);

        if (string.IsNullOrWhiteSpace(token))
        {
            return new ValidationResult
            {
                IsValid = false,
                Message = "Invalid or expired token"
            };
        }

        var claims = _tokenService.ValidateToken(token);
        
        if (claims == null)
        {
            return new ValidationResult
            {
                IsValid = false,
                Message = "Invalid or expired token"
            };
        }

        var userIdClaim = claims.FirstOrDefault(c => c.Type == "userId");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
        {
            return new ValidationResult
            {
                IsValid = false,
                Message = "Invalid token claims"
            };
        }

        var user = await _authProvider.GetUserByIdAsync(userId);
        
        if (user == null)
        {
            return new ValidationResult
            {
                IsValid = false,
                Message = "User not found"
            };
        }

        return new ValidationResult
        {
            IsValid = true,
            User = user,
            Message = "Token is valid",
            Claims = claims
        };
    }

    public async Task<ChangePasswordResult> ChangePasswordAsync(string username, string currentPassword, string newPassword)
    {
        // Sanitize inputs
        username = _inputSanitizer.SanitizeUsername(username);
        currentPassword = _inputSanitizer.SanitizePassword(currentPassword);
        newPassword = _inputSanitizer.SanitizePassword(newPassword);

        // Validate after sanitization
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
        {
            return new ChangePasswordResult
            {
                Success = false,
                Message = "Invalid input provided"
            };
        }

        // Check for malicious content
        if (_inputSanitizer.ContainsMaliciousContent(username) || 
            _inputSanitizer.ContainsMaliciousContent(currentPassword) || 
            _inputSanitizer.ContainsMaliciousContent(newPassword))
        {
            return new ChangePasswordResult
            {
                Success = false,
                Message = "Invalid input provided"
            };
        }

        var user = await _authProvider.GetUserByUsernameAsync(username);
        
        if (user == null)
        {
            return new ChangePasswordResult
            {
                Success = false,
                Message = "User not found"
            };
        }

        // Verify current password
        if (!_passwordHasher.VerifyPassword(currentPassword, user.Password))
        {
            return new ChangePasswordResult
            {
                Success = false,
                Message = "Current password is incorrect"
            };
        }

        // Validate new password
        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
        {
            return new ChangePasswordResult
            {
                Success = false,
                Message = "New password must be at least 6 characters long"
            };
        }

        // Hash new password
        var hashedNewPassword = _passwordHasher.HashPassword(newPassword);
        user.Password = hashedNewPassword;

        // Update user in database
        await _authProvider.UpdateUserAsync(user);

        return new ChangePasswordResult
        {
            Success = true,
            Message = "Password changed successfully"
        };
    }

    public async Task<SignUpResult> SignUpAsync(string username, string password)
    {
        // Sanitize inputs
        username = _inputSanitizer.SanitizeUsername(username);
        password = _inputSanitizer.SanitizePassword(password);

        // Validate username
        if (string.IsNullOrWhiteSpace(username) || username.Length < 3)
        {
            return new SignUpResult
            {
                Success = false,
                Message = "Username must be at least 3 characters long"
            };
        }

        // Validate password
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            return new SignUpResult
            {
                Success = false,
                Message = "Password must be at least 6 characters long"
            };
        }

        // Check for malicious content
        if (_inputSanitizer.ContainsMaliciousContent(username) || _inputSanitizer.ContainsMaliciousContent(password))
        {
            return new SignUpResult
            {
                Success = false,
                Message = "Invalid characters detected in username or password"
            };
        }

        // Check if username already exists
        var userExists = await _authProvider.UserExistsAsync(username);
        if (userExists)
        {
            return new SignUpResult
            {
                Success = false,
                Message = "Username already exists"
            };
        }

        // Hash password
        var hashedPassword = _passwordHasher.HashPassword(password);

        // Create new user
        var newUser = new User
        {
            Username = username,
            Password = hashedPassword
        };

        // Save user to database
        var createdUser = await _authProvider.CreateUserAsync(newUser);

        // Get user roles and permissions if authorization provider is available
        List<string>? roles = null;
        List<string>? permissions = null;
        if (_authorizationProvider != null)
        {
            roles = await _authorizationProvider.GetUserRolesAsync(createdUser.Username);
            
            // Get permissions directly from user (for database provider) or from roles (for mock provider)
            permissions = await _authorizationProvider.GetUserPermissionsAsync(createdUser.Username);
        }

        // Generate token for auto-login
        var token = _tokenService.GenerateToken(createdUser, roles, permissions);

        return new SignUpResult
        {
            Success = true,
            Message = "User registered successfully",
            Token = token,
            User = createdUser
        };
    }

    public Task<AuthorizationResult> AuthorizeRequestAsync(string token, string service,string controller, string action)
    {
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(controller) || string.IsNullOrWhiteSpace(action))
        {
            return Task.FromResult(new AuthorizationResult
            {
                IsAuthorized = false,
                Message = "Invalid request parameters"
            });
        }

        // Validate token
        var claims = _tokenService.ValidateToken(token);
        if (claims == null)
        {
            return Task.FromResult(new AuthorizationResult
            {
                IsAuthorized = false,
                Message = "Invalid or expired token"
            });
        }

        // Get username from token
        var usernameClaim = claims.FirstOrDefault(c => c.Type == "username");
        if (usernameClaim == null || string.IsNullOrEmpty(usernameClaim.Value))
        {
            return Task.FromResult(new AuthorizationResult
            {
                IsAuthorized = false,
                Message = "Username not found in token"
            });
        }

        // Build permission name: Controller.Action
        var requiredPermission = $"{service}.{controller}.{action}";

        // Get all permissions from token claims
        var tokenPermissions = claims
            .Where(c => c.Type == "permission")
            .Select(c => c.Value)
            .ToList();
        
        // Check authorization: user has "*" permission OR user has the specific permission
        var hasWildcardPermission = tokenPermissions.Contains("*");
        var hasSpecificPermission = tokenPermissions.Contains(requiredPermission);
        var isAuthorized = hasWildcardPermission || hasSpecificPermission;

        if (!isAuthorized)
        {
            return Task.FromResult(new AuthorizationResult
            {
                IsAuthorized = false,
                Message = $"User does not have permission for {requiredPermission}",
                Claims = claims
            });
        }

        return Task.FromResult(new AuthorizationResult
        {
            IsAuthorized = true,
            Message = "Request authorized",
            Claims = claims
        });
    }
}

