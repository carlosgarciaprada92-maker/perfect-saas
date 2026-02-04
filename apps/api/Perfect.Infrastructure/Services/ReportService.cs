using Microsoft.EntityFrameworkCore;
using Perfect.Application.Common;
using Perfect.Application.Contracts;
using Perfect.Application.Services;
using Perfect.Domain.Enums;
using Perfect.Infrastructure.Persistence;

namespace Perfect.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly AppDbContext _db;
    private readonly IInvoiceStatusCalculator _calculator;
    private readonly IDateTimeProvider _clock;

    public ReportService(AppDbContext db, IInvoiceStatusCalculator calculator, IDateTimeProvider clock)
    {
        _db = db;
        _calculator = calculator;
        _clock = clock;
    }

    public async Task<ReportSalesSummaryResponse> GetSalesSummaryAsync(int rangeDays, CancellationToken ct)
    {
        var cutoff = _clock.UtcNow.AddDays(-rangeDays);
        var query = _db.Invoices.Where(i => i.Date >= cutoff);
        var total = await query.SumAsync(i => i.Total, ct);
        var cash = await query.Where(i => i.PaymentType == PaymentType.Cash).SumAsync(i => i.Total, ct);
        var credit = await query.Where(i => i.PaymentType == PaymentType.Credit).SumAsync(i => i.Total, ct);

        return new ReportSalesSummaryResponse(total, cash, credit);
    }

    public async Task<IReadOnlyCollection<ReportTopCustomerResponse>> GetTopCustomersAsync(int rangeDays, int topN, CancellationToken ct)
    {
        var cutoff = _clock.UtcNow.AddDays(-rangeDays);

        var rows = await _db.Invoices
            .Include(i => i.Customer)
            .Where(i => i.CustomerId != null && i.Date >= cutoff)
            .GroupBy(i => new { i.CustomerId, CustomerName = i.Customer!.Name })
            .Select(g => new
            {
                CustomerId = g.Key.CustomerId!.Value,
                g.Key.CustomerName,
                Total = g.Sum(i => i.Total),
                Count = g.Count(),
                CreditTotal = g.Where(i => i.PaymentType == PaymentType.Credit).Sum(i => i.Total),
                CashTotal = g.Where(i => i.PaymentType == PaymentType.Cash).Sum(i => i.Total)
            })
            .OrderByDescending(x => x.Total)
            .Take(topN)
            .ToListAsync(ct);

        return rows.Select(x => new ReportTopCustomerResponse(
            x.CustomerId,
            x.CustomerName,
            x.Total,
            x.Count,
            x.Count == 0 ? 0 : x.Total / x.Count,
            x.Total == 0 ? 0 : (x.CreditTotal / x.Total) * 100,
            x.Total == 0 ? 0 : (x.CashTotal / x.Total) * 100
        )).ToList();
    }

    public async Task<IReadOnlyCollection<ReportCustomerActivityResponse>> GetCustomerActivityAsync(int inactiveDays, CancellationToken ct)
    {
        var cutoff = _clock.UtcNow.AddDays(-inactiveDays);
        var rows = await _db.Customers
            .Select(c => new
            {
                Customer = c,
                Last = c.Invoices.OrderByDescending(i => i.Date).Select(i => (DateTime?)i.Date).FirstOrDefault(),
                Total = c.Invoices.Sum(i => (decimal?)i.Total) ?? 0
            })
            .Where(x => !x.Last.HasValue || x.Last.Value < cutoff)
            .ToListAsync(ct);

        return rows.Select(x => new ReportCustomerActivityResponse(
            x.Customer.Id,
            x.Customer.Name,
            x.Last,
            x.Last.HasValue ? (_clock.UtcNow.Date - x.Last.Value.Date).Days : inactiveDays + 1,
            x.Total
        )).OrderByDescending(x => x.DaysInactive).ToList();
    }

    public async Task<ReportCreditKpiResponse> GetCreditKpisAsync(int rangeDays, int thresholdDays, CancellationToken ct)
    {
        var cutoff = _clock.UtcNow.AddDays(-rangeDays);
        var invoices = await _db.Invoices
            .Where(i => i.PaymentType == PaymentType.Credit && i.Date >= cutoff)
            .ToListAsync(ct);

        var stats = invoices.Select(i => new
        {
            Invoice = i,
            Status = _calculator.ResolveStatus(i.Balance, i.DueDate, _clock.UtcNow, thresholdDays)
        }).ToList();

        return new ReportCreditKpiResponse(
            stats.Where(x => x.Status != InvoiceStatus.Paid).Sum(x => x.Invoice.Balance),
            stats.Count(x => x.Status == InvoiceStatus.Overdue),
            stats.Count(x => x.Status == InvoiceStatus.DueSoon),
            stats.Where(x => x.Status == InvoiceStatus.Overdue).Sum(x => x.Invoice.Balance),
            stats.Where(x => x.Status == InvoiceStatus.DueSoon).Sum(x => x.Invoice.Balance)
        );
    }

    public async Task<IReadOnlyCollection<ReportSalesByDayResponse>> GetSalesByDayAsync(int rangeDays, CancellationToken ct)
    {
        var cutoff = _clock.UtcNow.Date.AddDays(-rangeDays);

        return await _db.Invoices
            .Where(i => i.Date >= cutoff)
            .GroupBy(i => i.Date.Date)
            .Select(g => new ReportSalesByDayResponse(
                g.Key,
                g.Count(),
                g.Sum(i => i.Total)))
            .OrderBy(x => x.Date)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<ReportSalesByPaymentTypeResponse>> GetSalesByPaymentTypeAsync(int rangeDays, CancellationToken ct)
    {
        var cutoff = _clock.UtcNow.Date.AddDays(-rangeDays);

        return await _db.Invoices
            .Where(i => i.Date >= cutoff)
            .GroupBy(i => i.PaymentType)
            .Select(g => new ReportSalesByPaymentTypeResponse(
                g.Key.ToString().ToUpperInvariant(),
                g.Count(),
                g.Sum(i => i.Total)))
            .OrderByDescending(x => x.Total)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyCollection<ReportOverdueCustomerResponse>> GetOverdueCustomersAsync(int topN, CancellationToken ct)
    {
        var rows = await _db.Invoices
            .Include(i => i.Customer)
            .Where(i => i.PaymentType == PaymentType.Credit && i.Balance > 0 && i.DueDate < _clock.UtcNow.Date && i.CustomerId != null)
            .Select(i => new
            {
                i.CustomerId,
                CustomerName = i.Customer!.Name,
                i.Balance,
                i.DueDate
            })
            .ToListAsync(ct);

        return rows
            .GroupBy(x => new { x.CustomerId, x.CustomerName })
            .Select(g => new ReportOverdueCustomerResponse(
                g.Key.CustomerId!.Value,
                g.Key.CustomerName,
                g.Count(),
                g.Sum(x => x.Balance),
                g.Max(x => (_clock.UtcNow.Date - x.DueDate.Date).Days),
                g.Max(x => x.DueDate)))
            .OrderByDescending(x => x.OverdueBalance)
            .Take(topN)
            .ToList();
    }
}
