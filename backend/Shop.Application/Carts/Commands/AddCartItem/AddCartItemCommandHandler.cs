using MediatR;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Common.Interfaces;
using Shop.Domain.Entities;

namespace Shop.Application.Carts.Commands.AddCartItem;

public class AddCartItemCommandHandler(IShopDbContext db) : IRequestHandler<AddCartItemCommand, Guid?>
{
    public async Task<Guid?> Handle(AddCartItemCommand request, CancellationToken cancellationToken)
    {
        var product = await db.Products
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product is null)
            return null;

        var cart = await db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == request.SessionId, cancellationToken);

        if (cart is null)
        {
            cart = Cart.Create(request.SessionId);
            db.Carts.Add(cart);
        }

        var cartItem = cart.AddOrUpdateItem(product.Id, product.Name, product.Price, request.Quantity);
        await db.SaveChangesAsync(cancellationToken);

        return cartItem.Id;
    }
}
