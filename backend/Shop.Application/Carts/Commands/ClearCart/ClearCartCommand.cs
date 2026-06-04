using MediatR;

namespace Shop.Application.Carts.Commands.ClearCart;

public record ClearCartCommand(Guid SessionId) : IRequest;
