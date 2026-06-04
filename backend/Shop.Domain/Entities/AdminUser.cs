namespace Shop.Domain.Entities;

public class AdminUser
{
    public Guid Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiry { get; private set; }

    private AdminUser() { }

    public static AdminUser Create(string username, string passwordHash) => new()
    {
        Id = Guid.NewGuid(),
        Username = username,
        PasswordHash = passwordHash
    };

    public void SetRefreshToken(string token, DateTime expiry)
    {
        RefreshToken = token;
        RefreshTokenExpiry = expiry;
    }

    public bool IsRefreshTokenValid(string token) =>
        RefreshToken == token && RefreshTokenExpiry > DateTime.UtcNow;
}
