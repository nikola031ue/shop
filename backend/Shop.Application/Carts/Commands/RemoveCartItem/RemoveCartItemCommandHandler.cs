using MediatR;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Common.Interfaces;

namespace Shop.Application.Carts.Commands.RemoveCartItem;

public class RemoveCartItemCommandHandler(IShopDbContext db) : IRequestHandler<RemoveCartItemCommand, bool>
{
    public async Task<bool> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        var cart = await db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == request.SessionId, cancellationToken);

        if (cart is null) return false;

        var removed = cart.RemoveItem(request.CartItemId);
        if (!removed) return false;

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
