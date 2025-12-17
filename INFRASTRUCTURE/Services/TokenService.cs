using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CORE.Entities;
using CORE.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace INFRASTRUCTURE.Services;

public class TokenService : ITokenService
{
    private readonly string _secretKey;
    private readonly JwtSecurityTokenHandler _tokenHandler;

    public TokenService(IConfiguration configuration)
    {
        _secretKey = configuration["JwtSecretKey"] 
            ?? throw new InvalidOperationException("JwtSecretKey is not configured");
        _tokenHandler = new JwtSecurityTokenHandler();
    }

    public string GenerateToken(User user, List<string>? roles = null, List<string>? permissions = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim("userId", user.Id.ToString()),
            new Claim("username", user.Username),
            new Claim(JwtRegisteredClaimNames.Sub, user.Username),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        // Add roles to claims
        if (roles != null && roles.Any())
        {
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        }

        // Add permissions to claims
        if (permissions != null && permissions.Any())
        {
            claims.AddRange(permissions.Select(permission => new Claim("permission", permission)));
        }

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24), // Token expires in 24 hours
            signingCredentials: credentials
        );

        return _tokenHandler.WriteToken(token);
    }

    public IEnumerable<Claim>? ValidateToken(string token)
    {
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = _tokenHandler.ValidateToken(token, validationParameters, out _);
            return principal.Claims;
        }
        catch
        {
            return null;
        }
    }
}

