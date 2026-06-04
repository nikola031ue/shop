using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Shop.Tests.Integration.Infrastructure;

namespace Shop.Tests.Integration.Products;

public class ProductsApiTests(ShopApiFactory factory) : IClassFixture<ShopApiFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    public Task InitializeAsync() => factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetAll_ShouldReturn200_WithEmptyList()
    {
        var response = await _client.GetAsync("/api/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("totalCount").GetInt32().Should().Be(0);
        body.GetProperty("items").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task Post_ShouldReturn201_AndCreateProduct()
    {
        var command = new { name = "Patike Nike", description = "Sportske patike", price = 89.99, stock = 50, imageUrl = (string?)null };

        var response = await _client.PostAsJsonAsync("/api/products", command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("id").GetGuid().Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetById_ShouldReturn200_WithProduct()
    {
        var id = await CreateProductAsync("Majica Adidas", 39.99m);

        var response = await _client.GetAsync($"/api/products/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("name").GetString().Should().Be("Majica Adidas");
        body.GetProperty("price").GetDecimal().Should().Be(39.99m);
    }

    [Fact]
    public async Task GetById_ShouldReturn404_WhenNotFound()
    {
        var response = await _client.GetAsync($"/api/products/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Put_ShouldReturn204_AndUpdateProduct()
    {
        var id = await CreateProductAsync("Stari naziv", 10m);
        var update = new { id, name = "Novi naziv", description = "Novi opis", price = 20.0, stock = 5, imageUrl = (string?)null };

        var response = await _client.PutAsJsonAsync($"/api/products/{id}", update);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/products/{id}");
        var body = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("name").GetString().Should().Be("Novi naziv");
    }

    [Fact]
    public async Task Put_ShouldReturn404_WhenNotFound()
    {
        var id = Guid.NewGuid();
        var update = new { id, name = "Naziv", description = "Opis", price = 10.0, stock = 1, imageUrl = (string?)null };

        var response = await _client.PutAsJsonAsync($"/api/products/{id}", update);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ShouldReturn204_AndRemoveProduct()
    {
        var id = await CreateProductAsync("Produkt za brisanje", 5m);

        var deleteResponse = await _client.DeleteAsync($"/api/products/{id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/products/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ShouldReturn404_WhenNotFound()
    {
        var response = await _client.DeleteAsync($"/api/products/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAll_ShouldFilterBySearchTerm()
    {
        await CreateProductAsync("Patike Nike Air", 100m);
        await CreateProductAsync("Majica Adidas", 40m);

        var response = await _client.GetAsync("/api/products?search=Nike");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("totalCount").GetInt32().Should().Be(1);
        body.GetProperty("items")[0].GetProperty("name").GetString().Should().Be("Patike Nike Air");
    }

    private async Task<Guid> CreateProductAsync(string name, decimal price)
    {
        var command = new { name, description = "Opis", price, stock = 10, imageUrl = (string?)null };
        var response = await _client.PostAsJsonAsync("/api/products", command);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }
}
