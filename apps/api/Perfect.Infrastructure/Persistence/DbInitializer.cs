using Microsoft.EntityFrameworkCore;
using Perfect.Domain.Entities;

namespace Perfect.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task EnsureCatalogAsync(AppDbContext db, CancellationToken ct = default)
    {
        if (!await db.Permissions.AnyAsync(ct))
        {
            db.Permissions.AddRange(PermissionCatalog.ToEntities());
            await db.SaveChangesAsync(ct);
        }
    }

    public static async Task<Tenant> EnsureTenantAsync(AppDbContext db, string name, string slug, CancellationToken ct)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();
        var existing = await db.Tenants.FirstOrDefaultAsync(x => x.Slug == normalizedSlug, ct);
        if (existing != null)
        {
            return existing;
        }

        var tenant = new Tenant { Name = name, Slug = normalizedSlug, Plan = "Demo" };
        db.Tenants.Add(tenant);
        db.TenantSettings.Add(new TenantSettings
        {
            TenantId = tenant.Id,
            DefaultCreditDays = 15,
            DueSoonThresholdDays = 5,
            LowStockThreshold = 5,
            AdministrativeFeePercent = 2m,
            Currency = "COP",
            Timezone = "America/Bogota",
            InvoiceNumberingFormat = "FAC-{0000}"
        });
        await db.SaveChangesAsync(ct);
        return tenant;
    }
}
