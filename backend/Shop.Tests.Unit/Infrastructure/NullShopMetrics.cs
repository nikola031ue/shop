using Shop.Application.Common.Interfaces;

namespace Shop.Tests.Unit.Infrastructure;

internal sealed class NullShopMetrics : IShopMetrics
{
    public static readonly NullShopMetrics Instance = new();
    public void RecordOrderCreated(decimal totalPrice) { }
    public void RecordCartItemAdded(int quantity) { }
}
