using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Shop.Tests.Integration.Infrastructure;

namespace Shop.Tests.Integration.Carts;

public class CartApiTests(ShopApiFactory factory) : IClassFixture<ShopApiFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();
    private readonly Guid _sessionId = Guid.NewGuid();

    public Task InitializeAsync() => factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetCart_ShouldReturn200_WithEmptyCart_WhenNoSession()
    {
        var response = await _client.GetAsync("/api/cart");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("items").GetArrayLength().Should().Be(0);
        body.GetProperty("total").GetDecimal().Should().Be(0);
    }

    [Fact]
    public async Task AddItem_ShouldReturn201_WhenProductExists()
    {
        var productId = await CreateProductAsync("Patike Nike", 89.99m);

        var response = await PostCartItemAsync(productId, 2);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task AddItem_ShouldReturn404_WhenProductNotFound()
    {
        var request = new { productId = Guid.NewGuid(), quantity = 1 };
        _client.DefaultRequestHeaders.Remove("X-Session-Id");
        _client.DefaultRequestHeaders.Add("X-Session-Id", _sessionId.ToString());

        var response = await _client.PostAsJsonAsync("/api/cart/items", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCart_ShouldReturnItems_AfterAdding()
    {
        var productId = await CreateProductAsync("Majica Adidas", 39.99m);
        await PostCartItemAsync(productId, 3);

        SetSessionHeader();
        var response = await _client.GetAsync("/api/cart");
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();

        body.GetProperty("items").GetArrayLength().Should().Be(1);
        body.GetProperty("total").GetDecimal().Should().Be(119.97m);
    }

    [Fact]
    public async Task AddSameProductTwice_ShouldIncreaseQuantity()
    {
        var productId = await CreateProductAsync("Kapa", 19.99m);
        await PostCartItemAsync(productId, 1);
        await PostCartItemAsync(productId, 2);

        SetSessionHeader();
        var cart = await _client.GetFromJsonAsync<JsonElement>("/api/cart");
        cart.GetProperty("items").GetArrayLength().Should().Be(1);
        cart.GetProperty("items")[0].GetProperty("quantity").GetInt32().Should().Be(3);
    }

    [Fact]
    public async Task UpdateItem_ShouldReturn204_AndChangeQuantity()
    {
        var productId = await CreateProductAsync("Patike", 90m);
        var cartItemId = await AddCartItemAsync(productId, 1);

        SetSessionHeader();
        var response = await _client.PutAsJsonAsync($"/api/cart/items/{cartItemId}", new { quantity = 5 });
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var cart = await _client.GetFromJsonAsync<JsonElement>("/api/cart");
        cart.GetProperty("items")[0].GetProperty("quantity").GetInt32().Should().Be(5);
    }

    [Fact]
    public async Task UpdateItem_ShouldRemoveItem_WhenQuantityIsZero()
    {
        var productId = await CreateProductAsync("Cipele", 110m);
        var cartItemId = await AddCartItemAsync(productId, 2);

        SetSessionHeader();
        await _client.PutAsJsonAsync($"/api/cart/items/{cartItemId}", new { quantity = 0 });

        var cart = await _client.GetFromJsonAsync<JsonElement>("/api/cart");
        cart.GetProperty("items").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task DeleteItem_ShouldReturn204_AndRemoveItem()
    {
        var productId = await CreateProductAsync("Torba", 55m);
        var cartItemId = await AddCartItemAsync(productId, 1);

        SetSessionHeader();
        var response = await _client.DeleteAsync($"/api/cart/items/{cartItemId}");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var cart = await _client.GetFromJsonAsync<JsonElement>("/api/cart");
        cart.GetProperty("items").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task DeleteItem_ShouldReturn404_WhenNotFound()
    {
        SetSessionHeader();
        var response = await _client.DeleteAsync($"/api/cart/items/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ClearCart_ShouldReturn204_AndEmptyCart()
    {
        var productId = await CreateProductAsync("Šal", 25m);
        await PostCartItemAsync(productId, 2);

        SetSessionHeader();
        var clearResponse = await _client.DeleteAsync("/api/cart");
        clearResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var cart = await _client.GetFromJsonAsync<JsonElement>("/api/cart");
        cart.GetProperty("items").GetArrayLength().Should().Be(0);
    }

    private void SetSessionHeader()
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

    private async Task<HttpResponseMessage> PostCartItemAsync(Guid productId, int quantity)
    {
        SetSessionHeader();
        return await _client.PostAsJsonAsync("/api/cart/items", new { productId, quantity });
    }

    private async Task<Guid> AddCartItemAsync(Guid productId, int quantity)
    {
        var response = await PostCartItemAsync(productId, quantity);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("cartItemId").GetGuid();
    }
}
