using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Auth.Dtos;
using Shop.Application.Auth.Interfaces;
using Shop.Application.Common.Interfaces;

namespace Shop.Application.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler(IShopDbContext db, IJwtService jwtService)
    : IRequestHandler<RefreshTokenCommand, TokenDto?>
{
    public async Task<TokenDto?> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var principal = jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal is null) return null;

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId)) return null;

        var user = await db.AdminUsers.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null || !user.IsRefreshTokenValid(request.RefreshToken))
            return null;

        var newAccessToken = jwtService.GenerateAccessToken(user.Id, user.Username, "Admin");
        var (newRefreshToken, refreshExpiry) = jwtService.GenerateRefreshToken();

        user.SetRefreshToken(newRefreshToken, refreshExpiry);
        await db.SaveChangesAsync(cancellationToken);

        return new TokenDto(newAccessToken, newRefreshToken, DateTime.UtcNow.AddMinutes(60));
    }
}
