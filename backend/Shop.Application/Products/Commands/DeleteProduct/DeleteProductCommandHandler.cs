using MediatR;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Common.Interfaces;

namespace Shop.Application.Products.Commands.DeleteProduct;

public class DeleteProductCommandHandler(IShopDbContext db)
    : IRequestHandler<DeleteProductCommand, bool>
{
    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product is null)
            return false;

        db.Products.Remove(product);
        await db.SaveChangesAsync(cancellationToken);

        return true;
    }
}
