using Microsoft.EntityFrameworkCore;
using Perfect.Application.Common;
using Perfect.Application.Contracts;
using Perfect.Application.Services;
using Perfect.Domain.Entities;
using Perfect.Infrastructure.Persistence;

namespace Perfect.Infrastructure.Services;

public class RoleService : IRoleService
{
    private readonly AppDbContext _db;

    public RoleService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyCollection<RoleResponse>> GetAsync(CancellationToken ct)
    {
        return await _db.Roles
            .OrderBy(x => x.Name)
            .Select(x => new RoleResponse(x.Id, x.Name, x.IsSystemRole))
            .ToListAsync(ct);
    }

    public async Task<RoleResponse> CreateAsync(RoleRequest request, CancellationToken ct)
    {
        var exists = await _db.Roles.AnyAsync(x => x.Name == request.Name, ct);
        if (exists)
        {
            throw new AppException(ErrorCodes.Conflict, "Role already exists", 409);
        }

        var role = new Role
        {
            Name = request.Name,
            IsSystemRole = false
        };

        _db.Roles.Add(role);
        await _db.SaveChangesAsync(ct);
        return new RoleResponse(role.Id, role.Name, role.IsSystemRole);
    }

    public async Task AssignPermissionsAsync(Guid id, AssignPermissionsRequest request, CancellationToken ct)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new AppException(ErrorCodes.NotFound, "Role not found", 404);

        var permissions = await _db.Permissions.Where(p => request.PermissionIds.Contains(p.Id)).ToListAsync(ct);
        if (permissions.Count != request.PermissionIds.Count)
        {
            throw new AppException(ErrorCodes.Validation, "One or more permissions not found");
        }

        var existing = _db.RolePermissions.Where(rp => rp.RoleId == role.Id);
        _db.RolePermissions.RemoveRange(existing);

        foreach (var permissionId in request.PermissionIds)
        {
            _db.RolePermissions.Add(new RolePermission
            {
                RoleId = role.Id,
                PermissionId = permissionId
            });
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyCollection<PermissionResponse>> GetPermissionsAsync(CancellationToken ct)
    {
        return await _db.Permissions
            .OrderBy(x => x.Module)
            .ThenBy(x => x.Action)
            .Select(x => new PermissionResponse(x.Id, x.Code, x.Module, x.Action, x.Description))
            .ToListAsync(ct);
    }
}
