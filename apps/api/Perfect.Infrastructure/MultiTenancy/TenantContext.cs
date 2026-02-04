using Perfect.Application.Common;

namespace Perfect.Infrastructure.MultiTenancy;

public class TenantContext : ITenantProvider
{
    public Guid? TenantId { get; private set; }
    public string? TenantSlug { get; private set; }

    public void SetTenant(Guid? tenantId, string? tenantSlug)
    {
        TenantId = tenantId;
        TenantSlug = tenantSlug;
    }
}
