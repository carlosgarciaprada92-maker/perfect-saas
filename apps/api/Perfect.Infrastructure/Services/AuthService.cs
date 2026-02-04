using Microsoft.EntityFrameworkCore;
using Perfect.Application.Common;
using Perfect.Application.Contracts;
using Perfect.Application.Services;
using Perfect.Domain.Entities;
using Perfect.Infrastructure.MultiTenancy;
using Perfect.Infrastructure.Persistence;

namespace Perfect.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly ITenantProvider _tenantProvider;
    private readonly TenantContext _tenantContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(
        AppDbContext db,
        ITenantProvider tenantProvider,
        TenantContext tenantContext,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _db = db;
        _tenantProvider = tenantProvider;
        _tenantContext = tenantContext;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var tenantId = _tenantProvider.TenantId;
        if (!tenantId.HasValue && !string.IsNullOrWhiteSpace(request.TenantSlug))
        {
            var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Slug == request.TenantSlug, ct);
            if (tenant == null)
            {
                throw new AppException(ErrorCodes.Unauthorized, "Tenant not found", 401);
            }
            tenantId = tenant.Id;
            _tenantContext.SetTenant(tenant.Id, tenant.Slug);
        }

        if (!tenantId.HasValue)
        {
            throw new AppException(ErrorCodes.TenantMissing, "Tenant is required", 400);
        }

        var user = await _db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, ct);

        if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new AppException(ErrorCodes.Unauthorized, "Invalid credentials", 401);
        }

        if (!user.IsActive)
        {
            throw new AppException(ErrorCodes.Forbidden, "User inactive", 403);
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission.Code))
            .Distinct()
            .ToList();

        var accessToken = _tokenService.GenerateAccessToken(new TokenDescriptor(user.Id, tenantId.Value, user.Email, roles, permissions));
        var refresh = _tokenService.GenerateRefreshToken();

        _db.RefreshTokens.Add(new RefreshToken
        {
            TenantId = tenantId.Value,
            UserId = user.Id,
            Token = refresh.Token,
            ExpiresAt = refresh.ExpiresAt,
            IsRevoked = false
        });
        await _db.SaveChangesAsync(ct);

        return new LoginResponse(accessToken, refresh.Token, refresh.ExpiresAt, new UserSummary(user.Id, user.Name, user.Email, roles, permissions));
    }

    public async Task<RefreshResponse> RefreshAsync(RefreshRequest request, CancellationToken ct)
    {
        var token = await _db.RefreshTokens
            .IgnoreQueryFilters()
            .Include(rt => rt.User)
            .ThenInclude(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, ct);

        if (token == null || token.IsRevoked || token.ExpiresAt <= DateTime.UtcNow)
        {
            throw new AppException(ErrorCodes.Unauthorized, "Invalid refresh token", 401);
        }

        _tenantContext.SetTenant(token.TenantId, null);

        var roles = token.User.UserRoles.Select(ur => ur.Role.Name).ToList();
        var permissions = token.User.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission.Code))
            .Distinct()
            .ToList();

        var accessToken = _tokenService.GenerateAccessToken(new TokenDescriptor(token.UserId, token.TenantId, token.User.Email, roles, permissions));
        var refresh = _tokenService.GenerateRefreshToken();

        token.IsRevoked = true;
        token.ReplacedByToken = refresh.Token;

        _db.RefreshTokens.Add(new RefreshToken
        {
            TenantId = token.TenantId,
            UserId = token.UserId,
            Token = refresh.Token,
            ExpiresAt = refresh.ExpiresAt,
            IsRevoked = false
        });

        await _db.SaveChangesAsync(ct);
        return new RefreshResponse(accessToken, refresh.Token, refresh.ExpiresAt);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct)
    {
        var token = await _db.RefreshTokens.IgnoreQueryFilters().FirstOrDefaultAsync(rt => rt.Token == refreshToken, ct);
        if (token == null)
        {
            return;
        }
        token.IsRevoked = true;
        await _db.SaveChangesAsync(ct);
    }
}
