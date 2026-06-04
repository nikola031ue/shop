using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Shop.Tests.Integration.Infrastructure;

namespace Shop.Tests.Integration.Auth;

public class AuthApiTests(ShopApiFactory factory) : IClassFixture<ShopApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Login_ShouldReturn200_WithTokens_WhenCredentialsAreValid()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { username = "admin", password = "Admin123!" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
        body.GetProperty("refreshToken").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_ShouldReturn401_WhenPasswordIsWrong()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { username = "admin", password = "WrongPassword!" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ShouldReturn401_WhenUserNotFound()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { username = "nonexistent", password = "Admin123!" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_ShouldReturn200_WithNewTokens()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login",
            new { username = "admin", password = "Admin123!" });
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var accessToken = loginBody.GetProperty("accessToken").GetString()!;
        var refreshToken = loginBody.GetProperty("refreshToken").GetString()!;

        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh",
            new { accessToken, refreshToken });

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await refreshResponse.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Refresh_ShouldReturn401_WhenRefreshTokenIsInvalid()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login",
            new { username = "admin", password = "Admin123!" });
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var accessToken = loginBody.GetProperty("accessToken").GetString()!;

        var response = await _client.PostAsJsonAsync("/api/auth/refresh",
            new { accessToken, refreshToken = "invalid-refresh-token" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminEndpoint_ShouldReturn401_WithoutToken()
    {
        var response = await _client.PostAsJsonAsync("/api/products",
            new { name = "Test", description = "Desc", price = 10.0, stock = 1, imageUrl = (string?)null });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AdminEndpoint_ShouldReturn201_WithValidToken()
    {
        var token = await factory.GetAdminTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _client.PostAsJsonAsync("/api/products",
            new { name = "Auth Test Product", description = "Desc", price = 10.0, stock = 1, imageUrl = (string?)null });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task AdminEndpoint_ShouldReturn401_WithExpiredOrInvalidToken()
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid.jwt.token");

        var response = await _client.PostAsJsonAsync("/api/products",
            new { name = "Test", description = "Desc", price = 10.0, stock = 1, imageUrl = (string?)null });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
