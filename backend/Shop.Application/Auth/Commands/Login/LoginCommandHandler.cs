using MediatR;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Auth.Dtos;
using Shop.Application.Auth.Interfaces;
using Shop.Application.Common.Interfaces;

namespace Shop.Application.Auth.Commands.Login;

public class LoginCommandHandler(IShopDbContext db, IJwtService jwtService, IPasswordHasher passwordHasher)
    : IRequestHandler<LoginCommand, TokenDto?>
{
    public async Task<TokenDto?> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await db.AdminUsers
            .FirstOrDefaultAsync(u => u.Username == request.Username, cancellationToken);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
            return null;

        var accessToken = jwtService.GenerateAccessToken(user.Id, user.Username, "Admin");
        var (refreshToken, refreshExpiry) = jwtService.GenerateRefreshToken();

        user.SetRefreshToken(refreshToken, refreshExpiry);
        await db.SaveChangesAsync(cancellationToken);

        var accessExpiry = DateTime.UtcNow.AddMinutes(60);
        return new TokenDto(accessToken, refreshToken, accessExpiry);
    }
}
