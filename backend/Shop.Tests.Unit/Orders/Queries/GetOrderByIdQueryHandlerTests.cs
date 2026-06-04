using FluentAssertions;
using Shop.Application.Orders.Queries.GetOrderById;
using Shop.Domain.Entities;
using Shop.Tests.Unit.Products;

namespace Shop.Tests.Unit.Orders.Queries;

public class GetOrderByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnOrder_WhenFound()
    {
        using var db = DbContextFactory.Create();
        var sessionId = Guid.NewGuid();
        var cart = Cart.Create(sessionId);
        cart.AddOrUpdateItem(Guid.NewGuid(), "Patike", 90m, 2);
        var order = Order.Create(sessionId, cart.Items, "0xtxhash", "0xwallet");
        db.Orders.Add(order);
        await db.SaveChangesAsync();

        var handler = new GetOrderByIdQueryHandler(db);
        var result = await handler.Handle(new GetOrderByIdQuery(order.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.TotalPrice.Should().Be(180m);
        result.TransactionHash.Should().Be("0xtxhash");
        result.Items.Should().HaveCount(1);
        result.Status.Should().Be("Pending");
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenNotFound()
    {
        using var db = DbContextFactory.Create();
        var handler = new GetOrderByIdQueryHandler(db);

        var result = await handler.Handle(new GetOrderByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }
}
