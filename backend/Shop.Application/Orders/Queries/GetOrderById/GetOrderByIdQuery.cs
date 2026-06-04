using MediatR;
using Shop.Application.Orders.Dtos;

namespace Shop.Application.Orders.Queries.GetOrderById;

public record GetOrderByIdQuery(Guid OrderId) : IRequest<OrderDto?>;
