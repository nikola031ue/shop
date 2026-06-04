using MediatR;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Common.Interfaces;
using Shop.Application.Products.Dtos;

namespace Shop.Application.Products.Queries.GetProductById;

public class GetProductByIdQueryHandler(IShopDbContext db)
    : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        return await db.Products
            .Where(p => p.Id == request.Id)
            .Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Price, p.Stock, p.ImageUrl, p.CreatedAt, p.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
