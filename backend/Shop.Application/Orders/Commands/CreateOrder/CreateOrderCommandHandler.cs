using MediatR;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Common.Interfaces;
using Shop.Domain.Entities;

namespace Shop.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler(IShopDbContext db) : IRequestHandler<CreateOrderCommand, Guid?>
{
    public async Task<Guid?> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var cart = await db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == request.SessionId, cancellationToken);

        if (cart is null || cart.Items.Count == 0)
            return null;

        var order = Order.Create(request.SessionId, cart.Items, request.TransactionHash, request.WalletAddress);
        db.Orders.Add(order);

        cart.Clear();
        await db.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}
