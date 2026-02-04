using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Perfect.Api.Authorization;
using Perfect.Infrastructure.MultiTenancy;
using Perfect.Infrastructure.Persistence;

namespace Perfect.Api.Middlewares;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _tenantMode;
    private readonly string _tenantIdHeader;
    private readonly string _tenantSlugHeader;

    public TenantResolutionMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;
        _tenantMode = (configuration["Tenant:Mode"] ?? "mixed").Trim().ToLowerInvariant();
        _tenantIdHeader = configuration["Tenant:HeaderName"] ?? "X-Tenant-Id";
        _tenantSlugHeader = configuration["Tenant:SlugHeaderName"] ?? "X-Tenant-Slug";
    }

    public async Task Invoke(HttpContext context, TenantContext tenantContext, AppDbContext db)
    {
        Guid? tenantId = null;
        string? tenantSlug = null;

        if (AllowsHeaderResolution() &&
            context.Request.Headers.TryGetValue(_tenantIdHeader, out var tenantIdValue) &&
            Guid.TryParse(tenantIdValue, out var parsedTenantId))
        {
            tenantId = parsedTenantId;
        }

        if (!tenantId.HasValue && AllowsHeaderResolution())
        {
            if (context.Request.Headers.TryGetValue(_tenantSlugHeader, out var tenantSlugValue))
            {
                tenantSlug = tenantSlugValue.ToString();
            }
            else if (AllowsSubdomainResolution())
            {
                tenantSlug = ResolveSubdomain(context.Request.Host.Host);
            }

            if (!string.IsNullOrWhiteSpace(tenantSlug))
            {
                var tenant = await db.Tenants.AsNoTracking().FirstOrDefaultAsync(t => t.Slug == tenantSlug, context.RequestAborted);
                if (tenant != null)
                {
                    tenantId = tenant.Id;
                    tenantSlug = tenant.Slug;
                }
            }
        }

        if (!tenantId.HasValue && AllowsClaimResolution())
        {
            var claim = context.User.FindFirstValue("tenantId");
            if (Guid.TryParse(claim, out var fromClaim))
            {
                tenantId = fromClaim;
            }
        }

        tenantContext.SetTenant(tenantId, tenantSlug);

        var requiresTenant = context.GetEndpoint()?.Metadata.GetMetadata<RequireTenantAttribute>() != null;
        var isAuthenticated = context.User.Identity?.IsAuthenticated == true;
        if (requiresTenant && isAuthenticated && !tenantId.HasValue)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { code = "tenant_missing", message = "Tenant context is required" });
            return;
        }

        await _next(context);
    }

    private static string? ResolveSubdomain(string host)
    {
        if (string.IsNullOrWhiteSpace(host) || host.Contains("localhost", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var parts = host.Split('.');
        return parts.Length >= 3 ? parts[0] : null;
    }

    private bool AllowsHeaderResolution() => _tenantMode is "mixed" or "header";
    private bool AllowsClaimResolution() => _tenantMode is "mixed" or "claim";
    private bool AllowsSubdomainResolution() => _tenantMode is "mixed" or "subdomain";
}
