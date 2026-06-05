using MediatR;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Common.Interfaces;
using Shop.Domain.Entities;

namespace Shop.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler(IShopDbContext db) : IRequestHandler<CreateOrderCommand, Guid?>
{
    public async Task<Guid?> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var cartItems = await db.CartItems
            .Where(i => i.CartId == request.SessionId)
            .ToListAsync(cancellationToken);

        if (cartItems.Count == 0) return null;

        var order = Order.Create(request.SessionId, cartItems, request.TransactionHash, request.WalletAddress);
        db.Orders.Add(order);
        db.CartItems.RemoveRange(cartItems);

        await db.SaveChangesAsync(cancellationToken);
        return order.Id;
    }
}
