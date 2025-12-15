using System.Security.Claims;

namespace CORE.Interfaces;

public interface ITokenService
{
    string GenerateToken(CORE.Entities.User user);
    IEnumerable<Claim>? ValidateToken(string token);
}

