using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Shop.Tests.Integration.Infrastructure;

namespace Shop.Tests.Integration.Orders;

public class OrderApiTests(ShopApiFactory factory) : IClassFixture<ShopApiFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly Guid _sessionId = Guid.NewGuid();

    public async Task InitializeAsync()
    {
        await factory.ResetDatabaseAsync();
        var token = await factory.GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetAll_ShouldReturn200_WithEmptyList()
    {
        var response = await _client.GetAsync("/api/orders");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("totalCount").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task Post_ShouldReturn400_WhenCartIsEmpty()
    {
        SetSession();
        var response = await _client.PostAsJsonAsync("/api/orders",
            new { transactionHash = "0xtx", walletAddress = "0xaddr" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_ShouldReturn400_WhenNoSessionHeader()
    {
        _client.DefaultRequestHeaders.Remove("X-Session-Id");
        var response = await _client.PostAsJsonAsync("/api/orders",
            new { transactionHash = "0xtx", walletAddress = "0xaddr" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Post_ShouldReturn201_AndCreateOrder_WithCartItems()
    {
        var productId = await CreateProductAsync("Patike Nike", 89.99m);
        await AddToCartAsync(productId, 2);

        SetSession();
        var response = await _client.PostAsJsonAsync("/api/orders",
            new { transactionHash = "0xabc123def", walletAddress = "0xwallet999" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("orderId").GetGuid().Should().NotBeEmpty();
    }

    [Fact]
    public async Task Post_ShouldClearCart_AfterOrderCreated()
    {
        var productId = await CreateProductAsync("Majica", 39.99m);
        await AddToCartAsync(productId, 1);

        SetSession();
        await _client.PostAsJsonAsync("/api/orders",
            new { transactionHash = "0xtx", walletAddress = "0xaddr" });

        var cart = await _client.GetFromJsonAsync<JsonElement>("/api/cart");
        cart.GetProperty("items").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task GetById_ShouldReturn200_WithCorrectData()
    {
        var productId = await CreateProductAsync("Cipele", 110m);
        await AddToCartAsync(productId, 1);

        SetSession();
        var orderId = await CreateOrderAsync();

        var response = await _client.GetAsync($"/api/orders/{orderId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("totalPrice").GetDecimal().Should().Be(110m);
        body.GetProperty("status").GetString().Should().Be("Pending");
        body.GetProperty("transactionHash").GetString().Should().Be("0xtxhash");
        body.GetProperty("items").GetArrayLength().Should().Be(1);
    }

    [Fact]
    public async Task GetById_ShouldReturn404_WhenNotFound()
    {
        var response = await _client.GetAsync($"/api/orders/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PutStatus_ShouldReturn204_AndUpdateStatus()
    {
        var productId = await CreateProductAsync("Torba", 55m);
        await AddToCartAsync(productId, 1);
        SetSession();
        var orderId = await CreateOrderAsync();

        var response = await _client.PutAsJsonAsync($"/api/orders/{orderId}/status",
            new { status = "Confirmed" });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var order = await _client.GetFromJsonAsync<JsonElement>($"/api/orders/{orderId}");
        order.GetProperty("status").GetString().Should().Be("Confirmed");
    }

    [Fact]
    public async Task PutStatus_ShouldReturn400_ForInvalidStatus()
    {
        var productId = await CreateProductAsync("Kapa", 25m);
        await AddToCartAsync(productId, 1);
        SetSession();
        var orderId = await CreateOrderAsync();

        var response = await _client.PutAsJsonAsync($"/api/orders/{orderId}/status",
            new { status = "NepostojeciStatus" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PutStatus_ShouldReturn404_WhenOrderNotFound()
    {
        var response = await _client.PutAsJsonAsync($"/api/orders/{Guid.NewGuid()}/status",
            new { status = "Confirmed" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private void SetSession()
    {
        _client.DefaultRequestHeaders.Remove("X-Session-Id");
        _client.DefaultRequestHeaders.Add("X-Session-Id", _sessionId.ToString());
    }

    private async Task<Guid> CreateProductAsync(string name, decimal price)
    {
        var response = await _client.PostAsJsonAsync("/api/products",
            new { name, description = "Opis", price, stock = 100, imageUrl = (string?)null });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }

    private async Task AddToCartAsync(Guid productId, int quantity)
    {
        SetSession();
        await _client.PostAsJsonAsync("/api/cart/items", new { productId, quantity });
    }

    private async Task<Guid> CreateOrderAsync()
    {
        SetSession();
        var response = await _client.PostAsJsonAsync("/api/orders",
            new { transactionHash = "0xtxhash", walletAddress = "0xwallet" });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("orderId").GetGuid();
    }
}
