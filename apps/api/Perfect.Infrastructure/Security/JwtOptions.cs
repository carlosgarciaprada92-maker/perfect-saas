namespace Perfect.Infrastructure.Security;

public class JwtOptions
{
    public string Issuer { get; set; } = "Perfect";
    public string Audience { get; set; } = "Perfect";
    public string Key { get; set; } = "super-secret-key";
    public string? SigningKey { get; set; }
    public int AccessTokenMinutes { get; set; } = 60;
    public int RefreshTokenDays { get; set; } = 7;

    public string ResolveSigningKey() => string.IsNullOrWhiteSpace(Key) ? SigningKey ?? string.Empty : Key;
}
