using MediatR;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Common.Interfaces;

namespace Shop.Application.Carts.Commands.RemoveCartItem;

public class RemoveCartItemCommandHandler(IShopDbContext db) : IRequestHandler<RemoveCartItemCommand, bool>
{
    public async Task<bool> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        var item = await db.CartItems
            .FirstOrDefaultAsync(i => i.Id == request.CartItemId && i.CartId == request.SessionId, cancellationToken);

        if (item is null) return false;

        db.CartItems.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
