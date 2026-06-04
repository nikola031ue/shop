using Microsoft.EntityFrameworkCore;
using Shop.Domain.Entities;

namespace Shop.Application.Common.Interfaces;

public interface IShopDbContext
{
    DbSet<Product> Products { get; }
    DbSet<Cart> Carts { get; }
    DbSet<CartItem> CartItems { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<AdminUser> AdminUsers { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
