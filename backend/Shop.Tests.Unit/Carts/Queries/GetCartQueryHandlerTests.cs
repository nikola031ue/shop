using FluentAssertions;
using Shop.Application.Carts.Queries.GetCart;
using Shop.Domain.Entities;
using Shop.Tests.Unit.Products;

namespace Shop.Tests.Unit.Carts.Queries;

public class GetCartQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnEmptyCart_WhenCartDoesNotExist()
    {
        using var db = DbContextFactory.Create();
        var handler = new GetCartQueryHandler(db);
        var sessionId = Guid.NewGuid();

        var result = await handler.Handle(new GetCartQuery(sessionId), CancellationToken.None);

        result.SessionId.Should().Be(sessionId);
        result.Items.Should().BeEmpty();
        result.Total.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldReturnCartWithItems_AndCalculateTotal()
    {
        using var db = DbContextFactory.Create();
        var sessionId = Guid.NewGuid();
        var cart = Cart.Create(sessionId);
        cart.AddOrUpdateItem(Guid.NewGuid(), "Patike", 90m, 2);
        cart.AddOrUpdateItem(Guid.NewGuid(), "Majica", 40m, 1);
        db.Carts.Add(cart);
        await db.SaveChangesAsync();

        var handler = new GetCartQueryHandler(db);

        var result = await handler.Handle(new GetCartQuery(sessionId), CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.Total.Should().Be(220m); // 90*2 + 40*1
    }
}
