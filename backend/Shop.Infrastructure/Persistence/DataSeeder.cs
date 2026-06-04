using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Shop.Application.Auth.Interfaces;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(ShopDbContext db, IConfiguration config, IPasswordHasher passwordHasher)
    {
        if (await db.AdminUsers.AnyAsync()) return;

        var username = config["Admin:Username"] ?? "admin";
        var password = config["Admin:Password"] ?? "Admin123!";

        db.AdminUsers.Add(AdminUser.Create(username, passwordHasher.Hash(password)));
        await db.SaveChangesAsync();
    }
}
