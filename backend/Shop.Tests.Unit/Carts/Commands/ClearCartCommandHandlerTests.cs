using FluentAssertions;
using Shop.Application.Carts.Commands.ClearCart;
using Shop.Domain.Entities;
using Shop.Tests.Unit.Products;

namespace Shop.Tests.Unit.Carts.Commands;

public class ClearCartCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldRemoveAllItems()
    {
        using var db = DbContextFactory.Create();
        var sessionId = Guid.NewGuid();
        var cart = Cart.Create(sessionId);
        cart.AddOrUpdateItem(Guid.NewGuid(), "Patike", 90m, 1);
        cart.AddOrUpdateItem(Guid.NewGuid(), "Majica", 40m, 2);
        db.Carts.Add(cart);
        await db.SaveChangesAsync();

        var handler = new ClearCartCommandHandler(db);
        await handler.Handle(new ClearCartCommand(sessionId), CancellationToken.None);

        db.CartItems.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldNotThrow_WhenCartDoesNotExist()
    {
        using var db = DbContextFactory.Create();
        var handler = new ClearCartCommandHandler(db);

        var act = async () => await handler.Handle(new ClearCartCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}
