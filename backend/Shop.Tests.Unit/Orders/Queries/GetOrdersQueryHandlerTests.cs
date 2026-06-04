using FluentAssertions;
using Shop.Application.Orders.Queries.GetOrders;
using Shop.Domain.Entities;
using Shop.Tests.Unit.Products;

namespace Shop.Tests.Unit.Orders.Queries;

public class GetOrdersQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoOrders()
    {
        using var db = DbContextFactory.Create();
        var handler = new GetOrdersQueryHandler(db);

        var result = await handler.Handle(new GetOrdersQuery(), CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldReturnOrdersWithPagination()
    {
        using var db = DbContextFactory.Create();
        for (var i = 0; i < 3; i++)
        {
            var sessionId = Guid.NewGuid();
            var cart = Cart.Create(sessionId);
            cart.AddOrUpdateItem(Guid.NewGuid(), $"Produkt {i}", 10m, 1);
            var order = Order.Create(sessionId, cart.Items, $"0xtx{i}", $"0xaddr{i}");
            db.Orders.Add(order);
        }
        await db.SaveChangesAsync();

        var handler = new GetOrdersQueryHandler(db);
        var result = await handler.Handle(new GetOrdersQuery(Page: 1, PageSize: 2), CancellationToken.None);

        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(2);
        result.HasNextPage.Should().BeTrue();
    }
}
