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

        await EnsureModuleCatalogAsync(db, ct);
    }

    public static async Task<Tenant> EnsureTenantAsync(AppDbContext db, string name, string slug, CancellationToken ct)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();
        var existing = await db.Tenants.FirstOrDefaultAsync(x => x.Slug == normalizedSlug, ct);
        if (existing != null)
        {
            if (string.IsNullOrWhiteSpace(existing.DisplayName))
            {
                existing.DisplayName = existing.Name;
                await db.SaveChangesAsync(ct);
            }
            return existing;
        }

        var tenant = new Tenant { Name = name, DisplayName = name, Slug = normalizedSlug, Plan = "Demo" };
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

    public static async Task EnsureModuleCatalogAsync(AppDbContext db, CancellationToken ct)
    {
        var seed = new[]
        {
            new ModuleCatalog
            {
                Name = "Peluquerías",
                Slug = "peluquerias",
                BaseUrl = "http://18.116.114.251",
                Status = Perfect.Domain.Enums.ModuleStatus.Active
            },
            new ModuleCatalog
            {
                Name = "Inventarios",
                Slug = "inventarios",
                BaseUrl = "http://18.116.114.251",
                Status = Perfect.Domain.Enums.ModuleStatus.Active
            }
        };

        foreach (var module in seed)
        {
            var exists = await db.ModuleCatalogs.AnyAsync(x => x.Slug == module.Slug, ct);
            if (!exists)
            {
                db.ModuleCatalogs.Add(module);
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
