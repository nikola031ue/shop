using MediatR;
using Shop.Application.Common.Models;
using Shop.Application.Orders.Dtos;

namespace Shop.Application.Orders.Queries.GetOrders;

public record GetOrdersQuery(int Page = 1, int PageSize = 20) : IRequest<PagedResult<OrderDto>>;
