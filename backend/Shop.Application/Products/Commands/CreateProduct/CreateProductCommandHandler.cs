using MediatR;
using Shop.Application.Common.Interfaces;
using Shop.Domain.Entities;

namespace Shop.Application.Products.Commands.CreateProduct;

public class CreateProductCommandHandler(IShopDbContext db)
    : IRequestHandler<CreateProductCommand, Guid>
{
    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = Product.Create(request.Name, request.Description, request.Price, request.Stock, request.ImageUrl);

        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}
