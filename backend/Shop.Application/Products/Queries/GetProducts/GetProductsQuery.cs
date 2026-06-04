using MediatR;
using Shop.Application.Common.Models;
using Shop.Application.Products.Dtos;

namespace Shop.Application.Products.Queries.GetProducts;

public record GetProductsQuery(string? SearchTerm, int Page = 1, int PageSize = 10)
    : IRequest<PagedResult<ProductDto>>;
