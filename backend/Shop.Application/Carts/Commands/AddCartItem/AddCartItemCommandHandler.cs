using MediatR;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Common.Interfaces;
using Shop.Domain.Entities;

namespace Shop.Application.Carts.Commands.AddCartItem;

public class AddCartItemCommandHandler(IShopDbContext db, IShopMetrics metrics)
    : IRequestHandler<AddCartItemCommand, Guid?>
{
    public async Task<Guid?> Handle(AddCartItemCommand request, CancellationToken cancellationToken)
    {
        var product = await db.Products
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken);

        if (product is null) return null;

        if (!await db.Carts.AnyAsync(c => c.Id == request.SessionId, cancellationToken))
        {
            db.Carts.Add(Cart.Create(request.SessionId));
            await db.SaveChangesAsync(cancellationToken);
        }

        var existing = await db.CartItems
            .FirstOrDefaultAsync(i => i.CartId == request.SessionId && i.ProductId == request.ProductId, cancellationToken);

        if (existing is not null)
        {
            existing.UpdateQuantity(existing.Quantity + request.Quantity);
            await db.SaveChangesAsync(cancellationToken);
            metrics.RecordCartItemAdded(request.Quantity);
            return existing.Id;
        }

        var cartItem = CartItem.Create(request.SessionId, product.Id, product.Name, product.Price, request.Quantity);
        db.CartItems.Add(cartItem);
        await db.SaveChangesAsync(cancellationToken);
        metrics.RecordCartItemAdded(request.Quantity);
        return cartItem.Id;
    }
}
