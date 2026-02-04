namespace Perfect.Application.Contracts;

public record LoginRequest(string Email, string Password, string? TenantSlug);
public record LoginResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt, UserSummary User);
public record RefreshRequest(string RefreshToken);
public record RefreshResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt);
public record LogoutRequest(string RefreshToken);
public record UserSummary(Guid Id, string Name, string Email, IReadOnlyCollection<string> Roles, IReadOnlyCollection<string> Permissions);
