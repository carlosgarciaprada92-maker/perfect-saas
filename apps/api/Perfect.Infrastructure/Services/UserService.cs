using Microsoft.EntityFrameworkCore;
using Perfect.Application.Common;
using Perfect.Application.Contracts;
using Perfect.Application.Models;
using Perfect.Application.Services;
using Perfect.Domain.Entities;
using Perfect.Infrastructure.Persistence;

namespace Perfect.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(AppDbContext db, IPasswordHasher passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    public async Task<PaginatedResult<UserResponse>> GetAsync(int page, int pageSize, string? search, CancellationToken ct)
    {
        var query = _db.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.Name.Contains(search) || x.Email.Contains(search));
        }

        var mapped = query.OrderBy(x => x.Name).Select(x => new UserResponse(x.Id, x.Name, x.Email, x.IsActive));
        return await mapped.ToPagedAsync(page, pageSize, ct);
    }

    public async Task<UserResponse> CreateAsync(UserRequest request, CancellationToken ct)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var exists = await _db.Users.AnyAsync(x => x.Email == normalizedEmail, ct);
        if (exists)
        {
            throw new AppException(ErrorCodes.Conflict, "Email already exists", 409);
        }

        var user = new User
        {
            Name = request.Name,
            Email = normalizedEmail,
            PasswordHash = _passwordHasher.Hash(request.Password),
            IsActive = request.IsActive
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        return new UserResponse(user.Id, user.Name, user.Email, user.IsActive);
    }

    public async Task<UserResponse> UpdateStatusAsync(Guid id, bool isActive, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new AppException(ErrorCodes.NotFound, "User not found", 404);

        user.IsActive = isActive;
        await _db.SaveChangesAsync(ct);
        return new UserResponse(user.Id, user.Name, user.Email, user.IsActive);
    }

    public async Task<UserResponse> UpdateAsync(Guid id, UserUpdateRequest request, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new AppException(ErrorCodes.NotFound, "User not found", 404);

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var emailExists = await _db.Users.AnyAsync(x => x.Id != id && x.Email == normalizedEmail, ct);
        if (emailExists)
        {
            throw new AppException(ErrorCodes.Conflict, "Email already exists", 409);
        }

        user.Name = request.Name;
        user.Email = normalizedEmail;
        user.IsActive = request.IsActive;
        await _db.SaveChangesAsync(ct);
        return new UserResponse(user.Id, user.Name, user.Email, user.IsActive);
    }

    public async Task AssignRolesAsync(Guid id, AssignRoleRequest request, CancellationToken ct)
    {
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new AppException(ErrorCodes.NotFound, "User not found", 404);

        var roles = await _db.Roles.Where(r => request.RoleIds.Contains(r.Id)).ToListAsync(ct);
        if (roles.Count != request.RoleIds.Count)
        {
            throw new AppException(ErrorCodes.Validation, "One or more roles not found");
        }

        var current = _db.UserRoles.Where(ur => ur.UserId == user.Id);
        _db.UserRoles.RemoveRange(current);
        foreach (var roleId in request.RoleIds)
        {
            _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleId });
        }

        await _db.SaveChangesAsync(ct);
    }
}
