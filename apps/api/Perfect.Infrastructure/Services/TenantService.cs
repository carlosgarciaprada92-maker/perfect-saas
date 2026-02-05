using Microsoft.EntityFrameworkCore;
using Perfect.Application.Common;
using Perfect.Application.Contracts;
using Perfect.Application.Services;
using Perfect.Domain.Entities;
using Perfect.Domain.Enums;
using Perfect.Infrastructure.Persistence;

namespace Perfect.Infrastructure.Services;

public class TenantService : ITenantService
{
    private readonly AppDbContext _db;
    private readonly ITenantProvider _tenantProvider;

    public TenantService(AppDbContext db, ITenantProvider tenantProvider)
    {
        _db = db;
        _tenantProvider = tenantProvider;
    }

    public async Task<TenantResponse> CreateTenantAsync(TenantCreateRequest request, CancellationToken ct)
    {
        var slug = request.Slug.Trim().ToLowerInvariant();
        var exists = await _db.Tenants.AnyAsync(t => t.Slug == slug, ct);
        if (exists)
        {
            throw new AppException(ErrorCodes.Conflict, "Tenant slug already exists", 409);
        }

        var tenant = new Tenant
        {
            Name = request.Name,
            DisplayName = request.Name,
            Slug = slug,
            Status = TenantStatus.Active,
            Plan = request.Plan,
            CreatedAt = DateTime.UtcNow
        };

        _db.Tenants.Add(tenant);
        _db.TenantSettings.Add(new TenantSettings
        {
            TenantId = tenant.Id
        });
        await _db.SaveChangesAsync(ct);

        return new TenantResponse(tenant.Id, tenant.Name, tenant.Slug, tenant.Status.ToString(), tenant.Plan, tenant.CreatedAt);
    }

    public async Task<TenantResponse> GetCurrentTenantAsync(CancellationToken ct)
    {
        if (!_tenantProvider.TenantId.HasValue)
        {
            throw new AppException(ErrorCodes.TenantMissing, "Tenant not resolved", 400);
        }

        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == _tenantProvider.TenantId.Value, ct)
            ?? throw new AppException(ErrorCodes.NotFound, "Tenant not found", 404);

        return new TenantResponse(tenant.Id, tenant.Name, tenant.Slug, tenant.Status.ToString(), tenant.Plan, tenant.CreatedAt);
    }

    public async Task<TenantSettingsResponse> UpdateSettingsAsync(TenantSettingsRequest request, CancellationToken ct)
    {
        if (!_tenantProvider.TenantId.HasValue)
        {
            throw new AppException(ErrorCodes.TenantMissing, "Tenant not resolved", 400);
        }

        var settings = await _db.TenantSettings.FirstOrDefaultAsync(x => x.TenantId == _tenantProvider.TenantId.Value, ct)
            ?? new TenantSettings { TenantId = _tenantProvider.TenantId.Value };

        settings.Currency = request.Currency;
        settings.Timezone = request.Timezone;
        settings.DefaultCreditDays = request.DefaultCreditDays;
        settings.DueSoonThresholdDays = request.DueSoonThresholdDays;
        settings.LowStockThreshold = request.LowStockThreshold;
        settings.AdministrativeFeePercent = request.AdministrativeFeePercent;
        settings.InvoiceNumberingFormat = request.InvoiceNumberingFormat;

        if (_db.Entry(settings).State == EntityState.Detached)
        {
            _db.TenantSettings.Add(settings);
        }

        await _db.SaveChangesAsync(ct);
        return new TenantSettingsResponse(
            settings.TenantId,
            settings.Currency,
            settings.Timezone,
            settings.DefaultCreditDays,
            settings.DueSoonThresholdDays,
            settings.LowStockThreshold,
            settings.AdministrativeFeePercent,
            settings.InvoiceNumberingFormat);
    }
}
