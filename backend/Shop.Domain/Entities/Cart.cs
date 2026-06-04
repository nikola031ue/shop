namespace Shop.Domain.Entities;

public class Cart
{
    public Guid Id { get; private set; }
    public List<CartItem> Items { get; private set; } = [];
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Cart() { }

    public static Cart Create(Guid sessionId) => new Cart
    {
        Id = sessionId,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    public CartItem AddOrUpdateItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        var existing = Items.FirstOrDefault(i => i.ProductId == productId);
        if (existing is not null)
        {
            existing.UpdateQuantity(existing.Quantity + quantity);
            UpdatedAt = DateTime.UtcNow;
            return existing;
        }

        var item = CartItem.Create(Id, productId, productName, unitPrice, quantity);
        Items.Add(item);
        UpdatedAt = DateTime.UtcNow;
        return item;
    }

    public bool RemoveItem(Guid cartItemId)
    {
        var item = Items.FirstOrDefault(i => i.Id == cartItemId);
        if (item is null) return false;
        Items.Remove(item);
        UpdatedAt = DateTime.UtcNow;
        return true;
    }

    public void Clear()
    {
        Items.Clear();
        UpdatedAt = DateTime.UtcNow;
    }
}
