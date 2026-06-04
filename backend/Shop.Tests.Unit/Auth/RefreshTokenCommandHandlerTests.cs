using System.Security.Claims;
using FluentAssertions;
using Moq;
using Shop.Application.Auth.Commands.RefreshToken;
using Shop.Application.Auth.Interfaces;
using Shop.Domain.Entities;
using Shop.Tests.Unit.Products;

namespace Shop.Tests.Unit.Auth;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IJwtService> _jwtService = new();

    [Fact]
    public async Task Handle_ShouldReturnNewToken_WhenRefreshTokenIsValid()
    {
        using var db = DbContextFactory.Create();
        var user = AdminUser.Create("admin", "hash");
        user.SetRefreshToken("valid-refresh", DateTime.UtcNow.AddDays(7));
        db.AdminUsers.Add(user);
        await db.SaveChangesAsync();

        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, "admin")
        ]));

        _jwtService.Setup(j => j.GetPrincipalFromExpiredToken("expired-token")).Returns(principal);
        _jwtService.Setup(j => j.GenerateAccessToken(user.Id, "admin", "Admin")).Returns("new-access-token");
        _jwtService.Setup(j => j.GenerateRefreshToken()).Returns(("new-refresh", DateTime.UtcNow.AddDays(7)));

        var handler = new RefreshTokenCommandHandler(db, _jwtService.Object);
        var result = await handler.Handle(new RefreshTokenCommand("expired-token", "valid-refresh"), CancellationToken.None);

        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("new-access-token");
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenRefreshTokenIsExpired()
    {
        using var db = DbContextFactory.Create();
        var user = AdminUser.Create("admin", "hash");
        user.SetRefreshToken("expired-refresh", DateTime.UtcNow.AddDays(-1));
        db.AdminUsers.Add(user);
        await db.SaveChangesAsync();

        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, "admin")
        ]));

        _jwtService.Setup(j => j.GetPrincipalFromExpiredToken("token")).Returns(principal);

        var handler = new RefreshTokenCommandHandler(db, _jwtService.Object);
        var result = await handler.Handle(new RefreshTokenCommand("token", "expired-refresh"), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenAccessTokenIsInvalid()
    {
        using var db = DbContextFactory.Create();
        _jwtService.Setup(j => j.GetPrincipalFromExpiredToken("bad-token")).Returns((ClaimsPrincipal?)null);

        var handler = new RefreshTokenCommandHandler(db, _jwtService.Object);
        var result = await handler.Handle(new RefreshTokenCommand("bad-token", "any-refresh"), CancellationToken.None);

        result.Should().BeNull();
    }
}
