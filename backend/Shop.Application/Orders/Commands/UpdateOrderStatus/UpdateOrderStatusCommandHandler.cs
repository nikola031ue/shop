using MediatR;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Common.Interfaces;

namespace Shop.Application.Orders.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommandHandler(IShopDbContext db) : IRequestHandler<UpdateOrderStatusCommand, bool>
{
    public async Task<bool> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
            return false;

        order.UpdateStatus(request.Status);
        await db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
