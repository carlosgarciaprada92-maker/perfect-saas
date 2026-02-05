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
        var defaultBaseUrl = ResolveDefaultModuleBaseUrl();
        const string legacyBaseUrl = "http://18.116.114.251";

        var seed = new[]
        {
            new ModuleCatalog
            {
                Name = "Peluquerías",
                Slug = "peluquerias",
                BaseUrl = BuildModuleUrl(defaultBaseUrl, "peluquerias"),
                Status = Perfect.Domain.Enums.ModuleStatus.Active
            },
            new ModuleCatalog
            {
                Name = "Inventarios",
                Slug = "inventarios",
                BaseUrl = BuildModuleUrl(defaultBaseUrl, "inventarios"),
                Status = Perfect.Domain.Enums.ModuleStatus.Active
            }
        };

        foreach (var module in seed)
        {
            var existing = await db.ModuleCatalogs.FirstOrDefaultAsync(x => x.Slug == module.Slug, ct);
            if (existing == null)
            {
                db.ModuleCatalogs.Add(module);
                continue;
            }

            var currentUrl = existing.BaseUrl?.Trim() ?? string.Empty;
            var desiredUrl = BuildModuleUrl(defaultBaseUrl, module.Slug);
            var hasLegacyUrl = string.Equals(currentUrl, legacyBaseUrl, StringComparison.OrdinalIgnoreCase);
            var isBaseOnly = string.Equals(NormalizeBaseUrl(currentUrl), NormalizeBaseUrl(defaultBaseUrl), StringComparison.OrdinalIgnoreCase);
            var shouldOverwrite = string.IsNullOrWhiteSpace(currentUrl) || hasLegacyUrl || isBaseOnly;

            if (shouldOverwrite && !string.IsNullOrWhiteSpace(desiredUrl))
            {
                existing.BaseUrl = desiredUrl;
            }
            else if (hasLegacyUrl && string.IsNullOrWhiteSpace(defaultBaseUrl))
            {
                existing.BaseUrl = string.Empty;
            }

            if (!string.Equals(existing.Name, module.Name, StringComparison.Ordinal))
            {
                existing.Name = module.Name;
            }

            if (existing.Status != module.Status)
            {
                existing.Status = module.Status;
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private static string ResolveDefaultModuleBaseUrl()
    {
        var value = Environment.GetEnvironmentVariable("CORE_DEFAULT_MODULE_BASEURL");
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    private static string BuildModuleUrl(string baseUrl, string slug)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return string.Empty;
        }

        var normalized = baseUrl.Trim().TrimEnd('/');
        return $"{normalized}/{slug}";
    }

    private static string NormalizeBaseUrl(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim().TrimEnd('/');
    }
}
