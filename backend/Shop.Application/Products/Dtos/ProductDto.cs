namespace Shop.Application.Products.Dtos;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int Stock,
    string? ImageUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt);
