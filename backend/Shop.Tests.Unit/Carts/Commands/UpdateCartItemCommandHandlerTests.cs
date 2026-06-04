using FluentAssertions;
using Shop.Application.Carts.Commands.UpdateCartItem;
using Shop.Domain.Entities;
using Shop.Tests.Unit.Products;

namespace Shop.Tests.Unit.Carts.Commands;

public class UpdateCartItemCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdateQuantity_AndReturnTrue()
    {
        using var db = DbContextFactory.Create();
        var sessionId = Guid.NewGuid();
        var cart = Cart.Create(sessionId);
        var item = cart.AddOrUpdateItem(Guid.NewGuid(), "Patike", 90m, 2);
        db.Carts.Add(cart);
        await db.SaveChangesAsync();

        var handler = new UpdateCartItemCommandHandler(db);
        var result = await handler.Handle(new UpdateCartItemCommand(sessionId, item.Id, 5), CancellationToken.None);

        result.Should().BeTrue();
        db.CartItems.Single().Quantity.Should().Be(5);
    }

    [Fact]
    public async Task Handle_ShouldRemoveItem_WhenQuantityIsZero()
    {
        using var db = DbContextFactory.Create();
        var sessionId = Guid.NewGuid();
        var cart = Cart.Create(sessionId);
        var item = cart.AddOrUpdateItem(Guid.NewGuid(), "Patike", 90m, 2);
        db.Carts.Add(cart);
        await db.SaveChangesAsync();

        var handler = new UpdateCartItemCommandHandler(db);
        await handler.Handle(new UpdateCartItemCommand(sessionId, item.Id, 0), CancellationToken.None);

        db.CartItems.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenCartItemNotFound()
    {
        using var db = DbContextFactory.Create();
        var handler = new UpdateCartItemCommandHandler(db);

        var result = await handler.Handle(new UpdateCartItemCommand(Guid.NewGuid(), Guid.NewGuid(), 1), CancellationToken.None);

        result.Should().BeFalse();
    }
}
