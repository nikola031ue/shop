using FluentAssertions;
using Shop.Application.Orders.Commands.CreateOrder;
using Shop.Domain.Entities;
using Shop.Domain.Enums;
using Shop.Tests.Unit.Products;

namespace Shop.Tests.Unit.Orders.Commands;

public class CreateOrderCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnNull_WhenCartDoesNotExist()
    {
        using var db = DbContextFactory.Create();
        var handler = new CreateOrderCommandHandler(db);

        var result = await handler.Handle(
            new CreateOrderCommand(Guid.NewGuid(), "0xabc", "0xwallet"), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenCartIsEmpty()
    {
        using var db = DbContextFactory.Create();
        var sessionId = Guid.NewGuid();
        db.Carts.Add(Cart.Create(sessionId));
        await db.SaveChangesAsync();

        var handler = new CreateOrderCommandHandler(db);
        var result = await handler.Handle(
            new CreateOrderCommand(sessionId, "0xabc", "0xwallet"), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldCreateOrder_WithCorrectTotalAndItems()
    {
        using var db = DbContextFactory.Create();
        var sessionId = Guid.NewGuid();
        var cart = Cart.Create(sessionId);
        cart.AddOrUpdateItem(Guid.NewGuid(), "Patike", 90m, 2);
        cart.AddOrUpdateItem(Guid.NewGuid(), "Majica", 40m, 1);
        db.Carts.Add(cart);
        await db.SaveChangesAsync();

        var handler = new CreateOrderCommandHandler(db);
        var orderId = await handler.Handle(
            new CreateOrderCommand(sessionId, "0xabc123", "0xwallet456"), CancellationToken.None);

        orderId.Should().NotBeNull();
        var order = db.Orders.First();
        order.TotalPrice.Should().Be(220m);
        order.Items.Should().HaveCount(2);
        order.Status.Should().Be(OrderStatus.Pending);
        order.TransactionHash.Should().Be("0xabc123");
    }

    [Fact]
    public async Task Handle_ShouldClearCart_AfterOrderCreated()
    {
        using var db = DbContextFactory.Create();
        var sessionId = Guid.NewGuid();
        var cart = Cart.Create(sessionId);
        cart.AddOrUpdateItem(Guid.NewGuid(), "Patike", 90m, 1);
        db.Carts.Add(cart);
        await db.SaveChangesAsync();

        var handler = new CreateOrderCommandHandler(db);
        await handler.Handle(
            new CreateOrderCommand(sessionId, "0xtx", "0xaddr"), CancellationToken.None);

        db.CartItems.Should().BeEmpty();
    }
}
