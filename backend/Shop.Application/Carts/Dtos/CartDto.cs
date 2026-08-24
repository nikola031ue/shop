namespace Shop.Application.Carts.Dtos;

public record CartItemDto(
    Guid CartItemId,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal Subtotal);

public record CartDto(
    Guid SessionId,
    IReadOnlyList<CartItemDto> Items,
    decimal Total);
