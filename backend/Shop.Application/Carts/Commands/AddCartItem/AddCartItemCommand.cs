using MediatR;

namespace Shop.Application.Carts.Commands.AddCartItem;

public record AddCartItemCommand(Guid SessionId, Guid ProductId, int Quantity) : IRequest<Guid?>;
