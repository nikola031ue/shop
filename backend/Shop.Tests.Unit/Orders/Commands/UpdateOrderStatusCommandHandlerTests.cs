using FluentAssertions;
using Shop.Application.Orders.Commands.UpdateOrderStatus;
using Shop.Domain.Entities;
using Shop.Domain.Enums;
using Shop.Tests.Unit.Products;

namespace Shop.Tests.Unit.Orders.Commands;

public class UpdateOrderStatusCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdateStatus_AndReturnTrue()
    {
        using var db = DbContextFactory.Create();
        var sessionId = Guid.NewGuid();
        var cart = Cart.Create(sessionId);
        cart.AddOrUpdateItem(Guid.NewGuid(), "Patike", 50m, 1);
        db.Carts.Add(cart);
        await db.SaveChangesAsync();

        var order = Order.Create(sessionId, cart.Items, "0xtx", "0xaddr");
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var handler = new UpdateOrderStatusCommandHandler(db);
        var result = await handler.Handle(
            new UpdateOrderStatusCommand(order.Id, OrderStatus.Confirmed), CancellationToken.None);

        result.Should().BeTrue();
        db.Orders.First().Status.Should().Be(OrderStatus.Confirmed);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenOrderNotFound()
    {
        using var db = DbContextFactory.Create();
        var handler = new UpdateOrderStatusCommandHandler(db);

        var result = await handler.Handle(
            new UpdateOrderStatusCommand(Guid.NewGuid(), OrderStatus.Confirmed), CancellationToken.None);

        result.Should().BeFalse();
    }
}
