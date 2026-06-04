using FluentAssertions;
using Shop.Application.Products.Queries.GetProductById;
using Shop.Domain.Entities;
using Shop.Tests.Unit.Products;

namespace Shop.Tests.Unit.Products.Queries;

public class GetProductByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnProduct_WhenFound()
    {
        using var db = DbContextFactory.Create();
        var product = Product.Create("Patike", "Opis", 50m, 10, "https://example.com/img.jpg");
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var handler = new GetProductByIdQueryHandler(db);

        var result = await handler.Handle(new GetProductByIdQuery(product.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
        result.Name.Should().Be("Patike");
        result.Price.Should().Be(50m);
    }

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenNotFound()
    {
        using var db = DbContextFactory.Create();
        var handler = new GetProductByIdQueryHandler(db);

        var result = await handler.Handle(new GetProductByIdQuery(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }
}
