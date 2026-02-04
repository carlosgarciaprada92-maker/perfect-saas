using Microsoft.EntityFrameworkCore;
using Perfect.Application.Models;

namespace Perfect.Infrastructure.Services;

internal static class QueryExtensions
{
    public static async Task<PaginatedResult<T>> ToPagedAsync<T>(this IQueryable<T> query, int page, int pageSize, CancellationToken ct)
    {
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PaginatedResult<T>(items, page, pageSize, total);
    }
}
