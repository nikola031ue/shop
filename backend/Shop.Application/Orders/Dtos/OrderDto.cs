namespace Shop.Application.Orders.Dtos;

public record OrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal Subtotal);

public record OrderDto(
    Guid Id,
    Guid SessionId,
    IReadOnlyList<OrderItemDto> Items,
    decimal TotalPrice,
    string TransactionHash,
    string WalletAddress,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt);
