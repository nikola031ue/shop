using FluentAssertions;
using Shop.Application.Products.Queries.GetProducts;
using Shop.Domain.Entities;
using Shop.Tests.Unit.Products;

namespace Shop.Tests.Unit.Products.Queries;

public class GetProductsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoProducts()
    {
        using var db = DbContextFactory.Create();
        var handler = new GetProductsQueryHandler(db);

        var result = await handler.Handle(new GetProductsQuery(null, 1, 10), CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllProducts_WithPagination()
    {
        using var db = DbContextFactory.Create();
        db.Products.AddRange(
            Product.Create("Produkt A", "Opis", 10m, 1, null),
            Product.Create("Produkt B", "Opis", 20m, 2, null),
            Product.Create("Produkt C", "Opis", 30m, 3, null));
        await db.SaveChangesAsync();

        var handler = new GetProductsQueryHandler(db);

        var result = await handler.Handle(new GetProductsQuery(null, 1, 2), CancellationToken.None);

        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(2);
        result.TotalPages.Should().Be(2);
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldFilterBySearchTerm()
    {
        using var db = DbContextFactory.Create();
        db.Products.AddRange(
            Product.Create("Patike Nike", "Sportske patike", 90m, 10, null),
            Product.Create("Majica Adidas", "Sportska majica", 40m, 20, null));
        await db.SaveChangesAsync();

        var handler = new GetProductsQueryHandler(db);

        var result = await handler.Handle(new GetProductsQuery("Nike", 1, 10), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items.Single().Name.Should().Be("Patike Nike");
    }
}
