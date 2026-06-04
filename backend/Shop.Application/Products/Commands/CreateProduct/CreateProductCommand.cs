using MediatR;

namespace Shop.Application.Products.Commands.CreateProduct;

public record CreateProductCommand(
    string Name,
    string Description,
    decimal Price,
    int Stock,
    string? ImageUrl) : IRequest<Guid>;
