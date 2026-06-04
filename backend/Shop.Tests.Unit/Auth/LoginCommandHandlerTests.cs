using FluentAssertions;
using Moq;
using Shop.Application.Auth.Commands.Login;
using Shop.Application.Auth.Interfaces;
using Shop.Domain.Entities;
using Shop.Tests.Unit.Products;

namespace Shop.Tests.Unit.Auth;

public class LoginCommandHandlerTests
{
    private readonly Mock<IJwtService> _jwtService = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();

    [Fact]
    public async Task Handle_ShouldReturnToken_WhenCredentialsAreValid()
    {
        using var db = DbContextFactory.Create();
        db.AdminUsers.Add(AdminUser.Create("admin", "hashed-password"));
        await db.SaveChangesAsync();

        _passwordHasher.Setup(p => p.Verify("Admin123!", "hashed-password")).Returns(true);
        _jwtService.Setup(j => j.GenerateAccessToken(It.IsAny<Guid>(), "admin", "Admin")).Returns("access-token");
        _jwtService.Setup(j => j.GenerateRefreshToken()).Returns(("refresh-token", DateTime.UtcNow.AddDays(7)));

        var handler = new LoginCommandHandler(db, _jwtService.Object, _passwordHasher.Object);

        var result = await handler.Handle(new LoginCommand("admin", "Admin123!"), CancellationToken.None);

        result.Should().NotBeNull();
        result!.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().Be("refresh-token");
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenUserNotFound()
    {
        using var db = DbContextFactory.Create();
        var handler = new LoginCommandHandler(db, _jwtService.Object, _passwordHasher.Object);

        var result = await handler.Handle(new LoginCommand("unknown", "password"), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenPasswordIsWrong()
    {
        using var db = DbContextFactory.Create();
        db.AdminUsers.Add(AdminUser.Create("admin", "hashed-password"));
        await db.SaveChangesAsync();

        _passwordHasher.Setup(p => p.Verify("wrongpassword", "hashed-password")).Returns(false);
        var handler = new LoginCommandHandler(db, _jwtService.Object, _passwordHasher.Object);

        var result = await handler.Handle(new LoginCommand("admin", "wrongpassword"), CancellationToken.None);

        result.Should().BeNull();
    }
}
