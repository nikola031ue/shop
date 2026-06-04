using FluentAssertions;
using Shop.Application.Carts.Commands.RemoveCartItem;
using Shop.Domain.Entities;
using Shop.Tests.Unit.Products;

namespace Shop.Tests.Unit.Carts.Commands;

public class RemoveCartItemCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldRemoveItem_AndReturnTrue()
    {
        using var db = DbContextFactory.Create();
        var sessionId = Guid.NewGuid();
        var cart = Cart.Create(sessionId);
        var item = cart.AddOrUpdateItem(Guid.NewGuid(), "Patike", 90m, 1);
        db.Carts.Add(cart);
        await db.SaveChangesAsync();

        var handler = new RemoveCartItemCommandHandler(db);
        var result = await handler.Handle(new RemoveCartItemCommand(sessionId, item.Id), CancellationToken.None);

        result.Should().BeTrue();
        db.CartItems.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenItemNotFound()
    {
        using var db = DbContextFactory.Create();
        var handler = new RemoveCartItemCommandHandler(db);

        var result = await handler.Handle(new RemoveCartItemCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        result.Should().BeFalse();
    }
}
