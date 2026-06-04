using MediatR;
using Shop.Domain.Enums;

namespace Shop.Application.Orders.Commands.UpdateOrderStatus;

public record UpdateOrderStatusCommand(Guid OrderId, OrderStatus Status) : IRequest<bool>;
