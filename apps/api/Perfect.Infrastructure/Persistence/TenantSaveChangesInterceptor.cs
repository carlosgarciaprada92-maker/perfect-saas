using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Perfect.Application.Common;
using Perfect.Domain.Common;

namespace Perfect.Infrastructure.Persistence;

public class TenantSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ITenantProvider _tenantProvider;
    private readonly IUserContext _userContext;
    private readonly IDateTimeProvider _clock;

    public TenantSaveChangesInterceptor(ITenantProvider tenantProvider, IUserContext userContext, IDateTimeProvider clock)
    {
        _tenantProvider = tenantProvider;
        _userContext = userContext;
        _clock = clock;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not DbContext context)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        var tenantId = _tenantProvider.TenantId;
        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added && entry.Entity is ITenantEntity tenantEntity)
            {
                if (tenantId.HasValue)
                {
                    tenantEntity.TenantId = tenantId.Value;
                }
            }

            if (entry.Entity is IAuditable auditable)
            {
                if (entry.State == EntityState.Added)
                {
                    auditable.CreatedAt = _clock.UtcNow;
                    auditable.CreatedByUserId = _userContext.UserId;
                }

                if (entry.State == EntityState.Modified)
                {
                    auditable.UpdatedAt = _clock.UtcNow;
                    auditable.UpdatedByUserId = _userContext.UserId;
                }
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
