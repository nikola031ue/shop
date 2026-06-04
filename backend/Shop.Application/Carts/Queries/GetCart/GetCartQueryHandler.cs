using MediatR;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Carts.Dtos;
using Shop.Application.Common.Interfaces;

namespace Shop.Application.Carts.Queries.GetCart;

public class GetCartQueryHandler(IShopDbContext db) : IRequestHandler<GetCartQuery, CartDto>
{
    public async Task<CartDto> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var cart = await db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == request.SessionId, cancellationToken);

        if (cart is null)
            return new CartDto(request.SessionId, [], 0);

        var items = cart.Items
            .Select(i => new CartItemDto(i.Id, i.ProductId, i.ProductName, i.UnitPrice, i.Quantity, i.UnitPrice * i.Quantity))
            .ToList();

        return new CartDto(cart.Id, items, items.Sum(i => i.Subtotal));
    }
}
