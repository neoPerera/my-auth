using CORE.Interfaces;
using CORE.Entities;

namespace APPLICATION.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(
        IUserRepository userRepository,
        ITokenService tokenService,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResult> AuthenticateAsync(string username, string password)
    {
        var user = await _userRepository.GetByUsernameAsync(username);
        
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

        var token = _tokenService.GenerateToken(user);

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

        var user = await _userRepository.GetByIdAsync(userId);
        
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
        var user = await _userRepository.GetByUsernameAsync(username);
        
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
        await _userRepository.UpdateAsync(user);

        return new ChangePasswordResult
        {
            Success = true,
            Message = "Password changed successfully"
        };
    }

    public async Task<SignUpResult> SignUpAsync(string username, string password)
    {
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

        // Check if username already exists
        var existingUser = await _userRepository.GetByUsernameAsync(username);
        if (existingUser != null)
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
        var createdUser = await _userRepository.CreateAsync(newUser);

        // Generate token for auto-login
        var token = _tokenService.GenerateToken(createdUser);

        return new SignUpResult
        {
            Success = true,
            Message = "User registered successfully",
            Token = token,
            User = createdUser
        };
    }
}

