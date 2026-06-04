using Shop.Domain.Enums;

namespace Shop.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public Guid SessionId { get; private set; }
    public List<OrderItem> Items { get; private set; } = [];
    public decimal TotalPrice { get; private set; }
    public string TransactionHash { get; private set; } = string.Empty;
    public string WalletAddress { get; private set; } = string.Empty;
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Order() { }

    public static Order Create(Guid sessionId, IEnumerable<CartItem> cartItems, string transactionHash, string walletAddress)
    {
        var items = cartItems
            .Select(ci => OrderItem.Create(ci.ProductId, ci.ProductName, ci.UnitPrice, ci.Quantity))
            .ToList();

        return new Order
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            Items = items,
            TotalPrice = items.Sum(i => i.UnitPrice * i.Quantity),
            TransactionHash = transactionHash,
            WalletAddress = walletAddress,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateStatus(OrderStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }
}
