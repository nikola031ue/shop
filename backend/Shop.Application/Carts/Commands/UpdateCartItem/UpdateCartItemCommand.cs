using MediatR;

namespace Shop.Application.Carts.Commands.UpdateCartItem;

public record UpdateCartItemCommand(Guid SessionId, Guid CartItemId, int Quantity) : IRequest<bool>;
