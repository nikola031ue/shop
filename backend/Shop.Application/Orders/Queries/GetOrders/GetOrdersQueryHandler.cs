using MediatR;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Common.Interfaces;
using Shop.Application.Common.Models;
using Shop.Application.Orders.Dtos;

namespace Shop.Application.Orders.Queries.GetOrders;

public class GetOrdersQueryHandler(IShopDbContext db) : IRequestHandler<GetOrdersQuery, PagedResult<OrderDto>>
{
    public async Task<PagedResult<OrderDto>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var total = await db.Orders.CountAsync(cancellationToken);

        var orders = await db.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => ToDto(o))
            .ToListAsync(cancellationToken);

        return new PagedResult<OrderDto>(orders, total, request.Page, request.PageSize);
    }

    private static OrderDto ToDto(Domain.Entities.Order o) =>
        new(o.Id, o.SessionId,
            o.Items.Select(i => new OrderItemDto(i.Id, i.ProductId, i.ProductName, i.UnitPrice, i.Quantity, i.UnitPrice * i.Quantity)).ToList(),
            o.TotalPrice, o.TransactionHash, o.WalletAddress, o.Status.ToString(),
            o.CreatedAt, o.UpdatedAt);
}
