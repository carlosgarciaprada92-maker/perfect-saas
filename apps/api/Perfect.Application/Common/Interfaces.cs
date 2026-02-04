namespace Perfect.Application.Common;

public interface ITenantProvider
{
    Guid? TenantId { get; }
    string? TenantSlug { get; }
}

public interface IUserContext
{
    Guid? UserId { get; }
    Guid? TenantId { get; }
    IReadOnlyCollection<string> Roles { get; }
    IReadOnlyCollection<string> Permissions { get; }
    bool HasPermission(string permission);
}

public interface IPasswordHasher
{
    string Hash(string input);
    bool Verify(string input, string hash);
}

public interface ITokenService
{
    string GenerateAccessToken(TokenDescriptor descriptor);
    RefreshTokenResult GenerateRefreshToken();
}

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}

public record TokenDescriptor(
    Guid UserId,
    Guid TenantId,
    string Email,
    IEnumerable<string> Roles,
    IEnumerable<string> Permissions
);

public record RefreshTokenResult(string Token, DateTime ExpiresAt);
