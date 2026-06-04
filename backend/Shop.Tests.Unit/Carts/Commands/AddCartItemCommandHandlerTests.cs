using FluentAssertions;
using Shop.Application.Carts.Commands.AddCartItem;
using Shop.Domain.Entities;
using Shop.Tests.Unit.Products;

namespace Shop.Tests.Unit.Carts.Commands;

public class AddCartItemCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnNull_WhenProductNotFound()
    {
        using var db = DbContextFactory.Create();
        var handler = new AddCartItemCommandHandler(db);

        var result = await handler.Handle(new AddCartItemCommand(Guid.NewGuid(), Guid.NewGuid(), 1), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldCreateCartAndAddItem_WhenCartDoesNotExist()
    {
        using var db = DbContextFactory.Create();
        var product = Product.Create("Patike", "Opis", 90m, 10, null);
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var sessionId = Guid.NewGuid();
        var handler = new AddCartItemCommandHandler(db);

        var cartItemId = await handler.Handle(new AddCartItemCommand(sessionId, product.Id, 2), CancellationToken.None);

        cartItemId.Should().NotBeNull();
        db.Carts.Should().HaveCount(1);
        db.CartItems.Single().Quantity.Should().Be(2);
        db.CartItems.Single().UnitPrice.Should().Be(90m);
    }

    [Fact]
    public async Task Handle_ShouldIncreaseQuantity_WhenSameProductAddedTwice()
    {
        using var db = DbContextFactory.Create();
        var product = Product.Create("Patike", "Opis", 90m, 10, null);
        db.Products.Add(product);
        var sessionId = Guid.NewGuid();
        var cart = Cart.Create(sessionId);
        cart.AddOrUpdateItem(product.Id, product.Name, product.Price, 1);
        db.Carts.Add(cart);
        await db.SaveChangesAsync();

        var handler = new AddCartItemCommandHandler(db);
        await handler.Handle(new AddCartItemCommand(sessionId, product.Id, 3), CancellationToken.None);

        db.CartItems.Single().Quantity.Should().Be(4);
    }
}
