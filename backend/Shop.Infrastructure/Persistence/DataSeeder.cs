using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Shop.Application.Auth.Interfaces;
using Shop.Domain.Entities;

namespace Shop.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(ShopDbContext db, IConfiguration config, IPasswordHasher passwordHasher)
    {
        await SeedAdminAsync(db, config, passwordHasher);
        await SeedProductsAsync(db);
    }

    private static async Task SeedAdminAsync(ShopDbContext db, IConfiguration config, IPasswordHasher passwordHasher)
    {
        if (await db.AdminUsers.AnyAsync()) return;

        var username = config["Admin:Username"] ?? "admin";
        var password = config["Admin:Password"] ?? "Admin123!";

        db.AdminUsers.Add(AdminUser.Create(username, passwordHasher.Hash(password)));
        await db.SaveChangesAsync();
    }

    private static async Task SeedProductsAsync(ShopDbContext db)
    {
        if (await db.Products.AnyAsync()) return;

        var products = new[]
        {
            Product.Create(
                "Nike Air Max 270",
                "Muške patike Nike Air Max 270 sa odličnom amortizacijom i modernim dizajnom. Idealne za svakodnevno nošenje.",
                89.99m, 50, null),

            Product.Create(
                "Adidas Originals Majica",
                "Klasična Adidas majica sa ikoničnim logotipom. Izrađena od 100% pamuka, dostupna u više boja.",
                39.99m, 100, null),

            Product.Create(
                "Levi's 501 Farmerke",
                "Originalne Levi's 501 farmerke sa ravnim krojem. Klasičan dizajn koji nikad ne izlazi iz mode.",
                79.99m, 40, null),

            Product.Create(
                "Ray-Ban Wayfarer Naočare",
                "Kultne Ray-Ban Wayfarer naočare za sunce sa UV400 zaštitom. Savršene za letnje dane.",
                149.99m, 25, null),

            Product.Create(
                "Nike Dri-FIT Dukserica",
                "Sportska dukserica Nike Dri-FIT izrađena od materijala koji odvodi vlagu. Idealna za trening.",
                65.00m, 60, null),

            Product.Create(
                "Adidas Ultraboost 22",
                "Premium Adidas Ultraboost tenisice sa Boost tehnologijom koja pruža izuzetnu udobnost pri trčanju.",
                119.99m, 30, null),

            Product.Create(
                "Puma Classic Kapa",
                "Puma klasična kapa sa vezenim logotipom. Savršena za hladne dane i sportske aktivnosti.",
                19.99m, 150, null),

            Product.Create(
                "Under Armour Ranac",
                "Prostrani Under Armour sportski ranac sa više pretinaca. Vodonepropusni materijal, kapacitet 25L.",
                55.00m, 35, null),
        };

        db.Products.AddRange(products);
        await db.SaveChangesAsync();
    }
}
