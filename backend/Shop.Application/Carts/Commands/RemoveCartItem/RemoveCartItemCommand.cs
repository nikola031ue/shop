using MediatR;

namespace Shop.Application.Carts.Commands.RemoveCartItem;

public record RemoveCartItemCommand(Guid SessionId, Guid CartItemId) : IRequest<bool>;
