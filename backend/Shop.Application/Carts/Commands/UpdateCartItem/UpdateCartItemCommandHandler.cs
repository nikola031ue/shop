using MediatR;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Common.Interfaces;

namespace Shop.Application.Carts.Commands.UpdateCartItem;

public class UpdateCartItemCommandHandler(IShopDbContext db) : IRequestHandler<UpdateCartItemCommand, bool>
{
    public async Task<bool> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
    {
        var item = await db.CartItems
            .FirstOrDefaultAsync(i => i.Id == request.CartItemId && i.CartId == request.SessionId, cancellationToken);

        if (item is null) return false;

        if (request.Quantity <= 0)
            db.CartItems.Remove(item);
        else
            item.UpdateQuantity(request.Quantity);

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
