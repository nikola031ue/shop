using MediatR;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Common.Interfaces;

namespace Shop.Application.Carts.Commands.ClearCart;

public class ClearCartCommandHandler(IShopDbContext db) : IRequestHandler<ClearCartCommand>
{
    public async Task Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        var items = await db.CartItems
            .Where(i => i.CartId == request.SessionId)
            .ToListAsync(cancellationToken);

        if (items.Count == 0) return;

        db.CartItems.RemoveRange(items);
        await db.SaveChangesAsync(cancellationToken);
    }
}
