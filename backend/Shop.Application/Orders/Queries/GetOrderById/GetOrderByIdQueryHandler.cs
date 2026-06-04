using MediatR;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Common.Interfaces;
using Shop.Application.Orders.Dtos;

namespace Shop.Application.Orders.Queries.GetOrderById;

public class GetOrderByIdQueryHandler(IShopDbContext db) : IRequestHandler<GetOrderByIdQuery, OrderDto?>
{
    public async Task<OrderDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var o = await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (o is null) return null;

        return new OrderDto(
            o.Id, o.SessionId,
            o.Items.Select(i => new OrderItemDto(i.Id, i.ProductId, i.ProductName, i.UnitPrice, i.Quantity, i.UnitPrice * i.Quantity)).ToList(),
            o.TotalPrice, o.TransactionHash, o.WalletAddress, o.Status.ToString(),
            o.CreatedAt, o.UpdatedAt);
    }
}
