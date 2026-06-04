using MediatR;
using Shop.Application.Products.Dtos;

namespace Shop.Application.Products.Queries.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<ProductDto?>;
