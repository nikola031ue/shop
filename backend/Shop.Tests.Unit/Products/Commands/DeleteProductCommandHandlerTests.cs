using FluentAssertions;
using Shop.Application.Products.Commands.DeleteProduct;
using Shop.Domain.Entities;
using Shop.Tests.Unit.Products;

namespace Shop.Tests.Unit.Products.Commands;

public class DeleteProductCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldDeleteProduct_AndReturnTrue_WhenFound()
    {
        using var db = DbContextFactory.Create();
        var product = Product.Create("Produkt", "Opis", 10m, 5, null);
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var handler = new DeleteProductCommandHandler(db);

        var result = await handler.Handle(new DeleteProductCommand(product.Id), CancellationToken.None);

        result.Should().BeTrue();
        db.Products.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenProductNotFound()
    {
        using var db = DbContextFactory.Create();
        var handler = new DeleteProductCommandHandler(db);

        var result = await handler.Handle(new DeleteProductCommand(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeFalse();
    }
}
