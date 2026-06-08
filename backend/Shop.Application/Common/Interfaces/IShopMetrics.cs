namespace Shop.Application.Common.Interfaces;

public interface IShopMetrics
{
    void RecordOrderCreated(decimal totalPrice);
    void RecordCartItemAdded(int quantity);
}
