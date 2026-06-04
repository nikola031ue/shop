namespace Shop.Application.Carts.Dtos;

public record CartItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal Subtotal);

public record CartDto(
    Guid SessionId,
    IReadOnlyList<CartItemDto> Items,
    decimal Total);
