using Microsoft.EntityFrameworkCore;
using Perfect.Application.Common;
using Perfect.Application.Contracts;
using Perfect.Application.Services;
using Perfect.Domain.Entities;
using Perfect.Domain.Enums;
using Perfect.Infrastructure.Persistence;

namespace Perfect.Infrastructure.Services;

public class PlatformService : IPlatformService
{
    private readonly AppDbContext _db;
    private readonly IDateTimeProvider _clock;

    public PlatformService(AppDbContext db, IDateTimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<IReadOnlyCollection<ModuleCatalogResponse>> GetModulesAsync(CancellationToken ct)
    {
        return await _db.ModuleCatalogs
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new ModuleCatalogResponse(x.Id, x.Name, x.Slug, x.BaseUrl, x.Icon, x.Status.ToString(), x.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<ModuleCatalogResponse> CreateModuleAsync(ModuleCatalogRequest request, CancellationToken ct)
    {
        var slug = request.Slug.Trim().ToLowerInvariant();
        var exists = await _db.ModuleCatalogs.AnyAsync(x => x.Slug == slug, ct);
        if (exists)
        {
            throw new AppException(ErrorCodes.Conflict, "Module slug already exists", 409);
        }

        var status = ParseModuleStatus(request.Status);
        var entity = new ModuleCatalog
        {
            Name = request.Name,
            Slug = slug,
            BaseUrl = request.BaseUrl,
            Icon = request.Icon,
            Status = status,
            CreatedAt = _clock.UtcNow
        };

        _db.ModuleCatalogs.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new ModuleCatalogResponse(entity.Id, entity.Name, entity.Slug, entity.BaseUrl, entity.Icon, entity.Status.ToString(), entity.CreatedAt);
    }

    public async Task<ModuleCatalogResponse> UpdateModuleAsync(Guid id, ModuleCatalogRequest request, CancellationToken ct)
    {
        var entity = await _db.ModuleCatalogs.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new AppException(ErrorCodes.NotFound, "Module not found", 404);

        var slug = request.Slug.Trim().ToLowerInvariant();
        if (!string.Equals(entity.Slug, slug, StringComparison.OrdinalIgnoreCase))
        {
            var exists = await _db.ModuleCatalogs.AnyAsync(x => x.Slug == slug, ct);
            if (exists)
            {
                throw new AppException(ErrorCodes.Conflict, "Module slug already exists", 409);
            }
        }

        var status = ParseModuleStatus(request.Status);
        entity.Name = request.Name;
        entity.Slug = slug;
        entity.BaseUrl = request.BaseUrl;
        entity.Icon = request.Icon;
        entity.Status = status;

        await _db.SaveChangesAsync(ct);

        return new ModuleCatalogResponse(entity.Id, entity.Name, entity.Slug, entity.BaseUrl, entity.Icon, entity.Status.ToString(), entity.CreatedAt);
    }

    public async Task<bool> DeleteModuleAsync(Guid id, CancellationToken ct)
    {
        var entity = await _db.ModuleCatalogs.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new AppException(ErrorCodes.NotFound, "Module not found", 404);

        _db.ModuleCatalogs.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyCollection<PlatformTenantResponse>> GetTenantsAsync(string? search, CancellationToken ct)
    {
        var query = _db.Tenants.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(x => x.Name.ToLower().Contains(term) || x.Slug.ToLower().Contains(term));
        }

        return await query
            .OrderBy(x => x.Name)
            .Select(x => new PlatformTenantResponse(x.Id, x.Name, x.DisplayName, x.Slug, x.Status.ToString(), x.Plan, x.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<PlatformTenantResponse> UpdateTenantStatusAsync(Guid id, TenantStatusUpdateRequest request, CancellationToken ct)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new AppException(ErrorCodes.NotFound, "Tenant not found", 404);

        tenant.Status = ParseTenantStatus(request.Status);
        await _db.SaveChangesAsync(ct);

        return new PlatformTenantResponse(tenant.Id, tenant.Name, tenant.DisplayName, tenant.Slug, tenant.Status.ToString(), tenant.Plan, tenant.CreatedAt);
    }

    public async Task<IReadOnlyCollection<ModuleAssignmentResponse>> GetAssignmentsAsync(Guid tenantId, CancellationToken ct)
    {
        var modules = await _db.ModuleCatalogs.AsNoTracking().OrderBy(x => x.Name).ToListAsync(ct);
        var assignments = await _db.TenantModules
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(ct);

        return modules
            .Select(module =>
            {
                var assignment = assignments.FirstOrDefault(x => x.ModuleId == module.Id);
                return new ModuleAssignmentResponse(
                    module.Id,
                    module.Name,
                    module.Slug,
                    module.BaseUrl,
                    module.Status.ToString(),
                    assignment?.Enabled ?? false,
                    assignment?.ActivatedAt,
                    assignment?.Notes);
            })
            .ToList();
    }

    public async Task<bool> UpdateAssignmentsAsync(ModuleAssignmentUpdateRequest request, CancellationToken ct)
    {
        var existing = await _db.TenantModules
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == request.TenantId)
            .ToListAsync(ct);

        foreach (var item in request.Modules)
        {
            var entity = existing.FirstOrDefault(x => x.ModuleId == item.ModuleId);
            if (entity == null)
            {
                entity = new TenantModule
                {
                    TenantId = request.TenantId,
                    ModuleId = item.ModuleId,
                    Enabled = item.Enabled,
                    ActivatedAt = item.Enabled ? _clock.UtcNow : null,
                    Notes = item.Notes
                };
                _db.TenantModules.Add(entity);
                continue;
            }

            entity.Enabled = item.Enabled;
            entity.Notes = item.Notes;
            if (item.Enabled && entity.ActivatedAt == null)
            {
                entity.ActivatedAt = _clock.UtcNow;
            }
        }

        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static ModuleStatus ParseModuleStatus(string status)
    {
        if (!Enum.TryParse<ModuleStatus>(status, true, out var parsed))
        {
            throw new AppException(ErrorCodes.Validation, "Invalid module status", 400);
        }
        return parsed;
    }

    private static TenantStatus ParseTenantStatus(string status)
    {
        if (!Enum.TryParse<TenantStatus>(status, true, out var parsed))
        {
            throw new AppException(ErrorCodes.Validation, "Invalid tenant status", 400);
        }
        return parsed;
    }
}
