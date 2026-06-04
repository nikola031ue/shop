using MediatR;
using Shop.Application.Carts.Dtos;

namespace Shop.Application.Carts.Queries.GetCart;

public record GetCartQuery(Guid SessionId) : IRequest<CartDto>;
