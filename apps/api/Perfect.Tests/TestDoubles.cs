using Perfect.Application.Common;

namespace Perfect.Tests;

internal sealed class TestTenantProvider : ITenantProvider
{
    public Guid? TenantId { get; set; }
    public string? TenantSlug { get; set; }
}

internal sealed class TestUserContext : IUserContext
{
    public Guid? UserId { get; set; } = Guid.NewGuid();
    public Guid? TenantId { get; set; }
    public IReadOnlyCollection<string> Roles { get; set; } = Array.Empty<string>();
    public IReadOnlyCollection<string> Permissions { get; set; } = Array.Empty<string>();

    public bool HasPermission(string permission) => Permissions.Contains(permission);
}

internal sealed class TestDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow { get; set; }
}