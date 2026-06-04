using MediatR;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Common.Interfaces;

namespace Shop.Application.Carts.Commands.UpdateCartItem;

public class UpdateCartItemCommandHandler(IShopDbContext db) : IRequestHandler<UpdateCartItemCommand, bool>
{
    public async Task<bool> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        var cart = await db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == request.SessionId, cancellationToken);

        if (cart is null) return false;

        var item = cart.Items.FirstOrDefault(i => i.Id == request.CartItemId);
        if (item is null) return false;

        if (request.Quantity <= 0)
            cart.RemoveItem(request.CartItemId);
        else
            item.UpdateQuantity(request.Quantity);

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
