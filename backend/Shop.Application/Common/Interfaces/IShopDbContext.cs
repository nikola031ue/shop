using Microsoft.EntityFrameworkCore;
using Shop.Domain.Entities;

namespace Shop.Application.Common.Interfaces;

public interface IShopDbContext
{
    DbSet<Product> Products { get; }
    DbSet<Cart> Carts { get; }
    DbSet<CartItem> CartItems { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
