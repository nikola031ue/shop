using System.Security.Claims;

namespace Shop.Application.Auth.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(Guid userId, string username, string role);
    (string Token, DateTime Expiry) GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
