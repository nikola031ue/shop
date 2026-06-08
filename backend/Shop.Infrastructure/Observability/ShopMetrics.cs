using System.Diagnostics.Metrics;
using Shop.Application.Common.Interfaces;

namespace Shop.Infrastructure.Observability;

public sealed class ShopMetrics : IShopMetrics
{
    public const string MeterName = "Shop.Api";

    private readonly Counter<long> _ordersCreated;
    private readonly Counter<double> _revenueUsd;
    private readonly Counter<long> _cartItemsAdded;

    public ShopMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName, "1.0.0");

        _ordersCreated = meter.CreateCounter<long>(
            "shop.orders.created",
            unit: "orders",
            description: "Broj kreiranih porudžbina");

        _revenueUsd = meter.CreateCounter<double>(
            "shop.revenue.usd",
            unit: "USD",
            description: "Ukupan prihod od porudžbina");

        _cartItemsAdded = meter.CreateCounter<long>(
            "shop.cart.items_added",
            unit: "items",
            description: "Broj artikala dodatih u korpu");
    }

    public void RecordOrderCreated(decimal totalPrice)
    {
        _ordersCreated.Add(1);
        _revenueUsd.Add((double)totalPrice);
    }

    public void RecordCartItemAdded(int quantity) =>
        _cartItemsAdded.Add(quantity);
}
