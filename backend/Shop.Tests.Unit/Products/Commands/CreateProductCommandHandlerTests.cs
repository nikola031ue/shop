using FluentAssertions;
using Shop.Application.Products.Commands.CreateProduct;
using Shop.Tests.Unit.Products;

namespace Shop.Tests.Unit.Products.Commands;

public class CreateProductCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateProduct_AndReturnGuid()
    {
        using var db = DbContextFactory.Create();
        var handler = new CreateProductCommandHandler(db);
        var command = new CreateProductCommand("Patike Nike", "Sportske patike", 89.99m, 50, "https://example.com/nike.jpg");

        var id = await handler.Handle(command, CancellationToken.None);

        id.Should().NotBeEmpty();
        db.Products.Should().HaveCount(1);
        db.Products.First().Name.Should().Be("Patike Nike");
        db.Products.First().Price.Should().Be(89.99m);
    }

    [Fact]
    public async Task Handle_ShouldSetCreatedAt()
    {
        using var db = DbContextFactory.Create();
        var handler = new CreateProductCommandHandler(db);
        var command = new CreateProductCommand("Test", "Desc", 10m, 1, null);

        await handler.Handle(command, CancellationToken.None);

        db.Products.First().CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
