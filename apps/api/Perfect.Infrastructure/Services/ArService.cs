using Microsoft.EntityFrameworkCore;
using Perfect.Application.Common;
using Perfect.Application.Contracts;
using Perfect.Application.Models;
using Perfect.Application.Services;
using Perfect.Domain.Entities;
using Perfect.Domain.Enums;
using Perfect.Infrastructure.Persistence;

namespace Perfect.Infrastructure.Services;

public class ArService : IArService
{
    private readonly AppDbContext _db;
    private readonly IInvoiceStatusCalculator _calculator;
    private readonly IDateTimeProvider _clock;

    public ArService(AppDbContext db, IInvoiceStatusCalculator calculator, IDateTimeProvider clock)
    {
        _db = db;
        _calculator = calculator;
        _clock = clock;
    }

    public async Task<ArSummaryResponse> GetSummaryAsync(int rangeDays, int thresholdDays, bool includePaid, Guid? customerId, CancellationToken ct)
    {
        var query = BuildBaseQuery(includePaid, customerId, rangeDays);
        var rows = await query.ToListAsync(ct);
        var items = rows.Select(x => ToArItem(x, thresholdDays)).ToList();
        return new ArSummaryResponse(
            items.Where(i => i.Status != "PAID").Sum(i => i.Balance),
            items.Count(i => i.Status == "OVERDUE"),
            items.Count(i => i.Status == "DUE_SOON")
        );
    }

    public async Task<IReadOnlyCollection<ArItemResponse>> GetDueSoonAsync(int thresholdDays, CancellationToken ct)
    {
        var rows = await BuildBaseQuery(false, null, null).ToListAsync(ct);
        return rows.Select(x => ToArItem(x, thresholdDays)).Where(x => x.Status == "DUE_SOON").OrderBy(x => x.DaysToDue).ToList();
    }

    public async Task<IReadOnlyCollection<ArItemResponse>> GetOverdueAsync(CancellationToken ct)
    {
        var threshold = await GetThresholdDaysAsync(ct);
        var rows = await BuildBaseQuery(false, null, null, null).ToListAsync(ct);
        return rows.Select(x => ToArItem(x, threshold)).Where(x => x.Status == "OVERDUE").OrderByDescending(x => x.DaysOverdue).ToList();
    }

    public async Task<PaginatedResult<ArItemResponse>> GetOpenItemsAsync(
        int page,
        int pageSize,
        string? status,
        bool includePaid,
        Guid? customerId,
        int thresholdDays,
        string? search,
        int? rangeDays,
        CancellationToken ct)
    {
        var rows = await BuildBaseQuery(includePaid, customerId, rangeDays, search).ToListAsync(ct);
        var threshold = thresholdDays > 0 ? thresholdDays : await GetThresholdDaysAsync(ct);
        var items = rows.Select(x => ToArItem(x, threshold)).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            items = items.Where(x => x.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        }

        var total = items.Count();
        var paged = items.OrderBy(x => x.DueDate).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return new PaginatedResult<ArItemResponse>(paged, page, pageSize, total);
    }

    private IQueryable<Invoice> BuildBaseQuery(bool includePaid, Guid? customerId, int? rangeDays)
    {
        return BuildBaseQuery(includePaid, customerId, rangeDays, null);
    }

    private IQueryable<Invoice> BuildBaseQuery(bool includePaid, Guid? customerId, int? rangeDays, string? search)
    {
        var query = _db.Invoices.Include(i => i.Customer).Where(i => i.PaymentType == PaymentType.Credit);

        if (!includePaid)
        {
            query = query.Where(i => i.Balance > 0);
        }

        if (customerId.HasValue)
        {
            query = query.Where(i => i.CustomerId == customerId.Value);
        }

        if (rangeDays.HasValue)
        {
            var cutoff = _clock.UtcNow.AddDays(-rangeDays.Value);
            query = query.Where(i => i.Date >= cutoff);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(i =>
                i.Number.Contains(term) ||
                (i.Customer != null && i.Customer.Name.Contains(term)));
        }

        return query;
    }

    private ArItemResponse ToArItem(Invoice invoice, int thresholdDays)
    {
        var status = _calculator.ResolveStatus(invoice.Balance, invoice.DueDate, _clock.UtcNow, thresholdDays);
        var daysToDue = (invoice.DueDate.Date - _clock.UtcNow.Date).Days;
        return new ArItemResponse(
            invoice.Id,
            invoice.Number,
            invoice.CustomerId,
            invoice.Customer?.Name,
            invoice.DueDate,
            Math.Max(0, daysToDue),
            daysToDue < 0 ? Math.Abs(daysToDue) : 0,
            invoice.Total,
            invoice.Balance,
            status.ToString().ToUpperInvariant()
        );
    }

    private async Task<int> GetThresholdDaysAsync(CancellationToken ct)
    {
        var threshold = await _db.TenantSettings
            .Select(x => (int?)x.DueSoonThresholdDays)
            .FirstOrDefaultAsync(ct);
        return Math.Clamp(threshold ?? 5, 1, 30);
    }
}
