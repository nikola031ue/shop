using FluentAssertions;
using Shop.Application.Products.Commands.UpdateProduct;
using Shop.Domain.Entities;
using Shop.Tests.Unit.Products;

namespace Shop.Tests.Unit.Products.Commands;

public class UpdateProductCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldUpdateProduct_AndReturnTrue_WhenFound()
    {
        using var db = DbContextFactory.Create();
        var product = Product.Create("Stari naziv", "Stari opis", 10m, 5, null);
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var handler = new UpdateProductCommandHandler(db);
        var command = new UpdateProductCommand(product.Id, "Novi naziv", "Novi opis", 20m, 10, null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeTrue();
        db.Products.First().Name.Should().Be("Novi naziv");
        db.Products.First().Price.Should().Be(20m);
    }

    [Fact]
    public async Task Handle_ShouldReturnFalse_WhenProductNotFound()
    {
        using var db = DbContextFactory.Create();
        var handler = new UpdateProductCommandHandler(db);
        var command = new UpdateProductCommand(Guid.NewGuid(), "Naziv", "Opis", 10m, 1, null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().BeFalse();
    }
}
