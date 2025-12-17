using System.Security.Claims;

namespace CORE.Interfaces;

public interface ITokenService
{
    string GenerateToken(CORE.Entities.User user, List<string>? roles = null, List<string>? permissions = null);
    IEnumerable<Claim>? ValidateToken(string token);
}

