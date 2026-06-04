using Microsoft.EntityFrameworkCore;
using Shop.Infrastructure.Persistence;

namespace Shop.Tests.Unit.Products;

internal static class DbContextFactory
{
    public static ShopDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ShopDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ShopDbContext(options);
    }
}
