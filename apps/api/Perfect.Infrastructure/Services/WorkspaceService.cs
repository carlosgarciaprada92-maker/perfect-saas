using Microsoft.EntityFrameworkCore;
using Perfect.Application.Common;
using Perfect.Application.Contracts;
using Perfect.Application.Services;
using Perfect.Domain.Entities;
using Perfect.Infrastructure.Persistence;

namespace Perfect.Infrastructure.Services;

public class WorkspaceService : IWorkspaceService
{
    private readonly AppDbContext _db;
    private readonly ITenantProvider _tenantProvider;

    public WorkspaceService(AppDbContext db, ITenantProvider tenantProvider)
    {
        _db = db;
        _tenantProvider = tenantProvider;
    }

    public async Task<IReadOnlyCollection<WorkspaceAppResponse>> GetAppsAsync(CancellationToken ct)
    {
        if (!_tenantProvider.TenantId.HasValue)
        {
            throw new AppException(ErrorCodes.TenantMissing, "Tenant not resolved", 400);
        }

        var modules = await _db.TenantModules
            .Include(x => x.Module)
            .Where(x => x.Enabled)
            .OrderBy(x => x.Module.Name)
            .ToListAsync(ct);

        return modules
            .Select(x => new WorkspaceAppResponse(
                x.ModuleId,
                x.Module.Name,
                x.Module.Slug,
                x.Module.BaseUrl,
                ResolveLaunchUrl(x.Module),
                x.Module.Status.ToString(),
                x.Enabled))
            .ToList();
    }

    public Task<IReadOnlyCollection<WorkspaceUserResponse>> GetUsersAsync(CancellationToken ct)
    {
        return Task.FromResult<IReadOnlyCollection<WorkspaceUserResponse>>(Array.Empty<WorkspaceUserResponse>());
    }

    private static string ResolveLaunchUrl(ModuleCatalog module)
    {
        if (!string.IsNullOrWhiteSpace(module.LaunchUrl))
        {
            return module.LaunchUrl.Trim();
        }

        return module.BaseUrl?.Trim() ?? string.Empty;
    }
}
