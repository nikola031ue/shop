using MediatR;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Common.Interfaces;

namespace Shop.Application.Carts.Commands.ClearCart;

public class ClearCartCommandHandler(IShopDbContext db) : IRequestHandler<ClearCartCommand>
{
    public async Task Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == request.SessionId, cancellationToken);

        if (cart is null) return;

        cart.Clear();
        await db.SaveChangesAsync(cancellationToken);
    }
}
