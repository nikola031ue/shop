using MediatR;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Common.Interfaces;

namespace Shop.Application.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler(IShopDbContext db)
    : IRequestHandler<UpdateProductCommand, bool>
{
    public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product is null)
            return false;

        product.Update(request.Name, request.Description, request.Price, request.Stock, request.ImageUrl);
        await db.SaveChangesAsync(cancellationToken);

        return true;
    }
}
